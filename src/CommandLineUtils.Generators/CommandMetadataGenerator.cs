// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Source generator that creates ICommandMetadataProvider implementations
    /// for classes marked with the [Command] attribute.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed class CommandMetadataGenerator : IIncrementalGenerator
    {
        private const string CommandAttributeFullName = "McMaster.Extensions.CommandLineUtils.CommandAttribute";
        private const string OptionAttributeFullName = "McMaster.Extensions.CommandLineUtils.OptionAttribute";
        private const string ArgumentAttributeFullName = "McMaster.Extensions.CommandLineUtils.ArgumentAttribute";
        private const string SubcommandAttributeFullName = "McMaster.Extensions.CommandLineUtils.SubcommandAttribute";
        private const string HelpOptionAttributeFullName = "McMaster.Extensions.CommandLineUtils.HelpOptionAttribute";
        private const string VersionOptionAttributeFullName = "McMaster.Extensions.CommandLineUtils.VersionOptionAttribute";
        private const string VersionOptionFromMemberAttributeFullName = "McMaster.Extensions.CommandLineUtils.VersionOptionFromMemberAttribute";

        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source (always available for reference)
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("CommandMetadataGeneratorAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8));
            });

            // Check if PublishAot is enabled - only generate metadata when AOT is on
            var isAotEnabled = context.AnalyzerConfigOptionsProvider
                .Select(static (options, _) =>
                {
                    options.GlobalOptions.TryGetValue("build_property.PublishAot", out var publishAot);
                    return string.Equals(publishAot, "true", StringComparison.OrdinalIgnoreCase);
                });

            // Find all classes with [Command] attribute
            var commandClasses = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    CommandAttributeFullName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, ct) => GetCommandData(ctx, ct))
                .Where(static m => m is not null)
                .Select(static (m, _) => m!);

            // Combine command classes with AOT flag
            var commandsWithAot = commandClasses.Combine(isAotEnabled);

            // Generate the metadata providers (only if AOT is enabled)
            context.RegisterSourceOutput(commandsWithAot, static (spc, tuple) =>
            {
                var (commandInfo, isAot) = tuple;
                if (!isAot) return; // Skip generation for non-AOT builds

                var source = GenerateMetadataProvider(commandInfo);
                spc.AddSource($"{commandInfo.FullTypeName.Replace(".", "_")}_Metadata.g.cs", SourceText.From(source, Encoding.UTF8));
            });

            // Generate the module initializer that registers all providers (only if AOT is enabled)
            var allCommandsWithAot = commandClasses.Collect().Combine(isAotEnabled);
            context.RegisterSourceOutput(allCommandsWithAot, static (spc, tuple) =>
            {
                var (commands, isAot) = tuple;
                if (!isAot || commands.Length == 0)
                {
                    return;
                }

                var source = GenerateModuleInitializer(commands);
                spc.AddSource("CommandMetadataRegistration.g.cs", SourceText.From(source, Encoding.UTF8));
            });
        }

        private static CommandData? GetCommandData(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            {
                return null;
            }

            var classDeclaration = (ClassDeclarationSyntax)context.TargetNode;

            // Get command attribute data
            var commandAttr = context.Attributes.FirstOrDefault(a =>
                a.AttributeClass?.ToDisplayString() == CommandAttributeFullName);

            if (commandAttr is null)
            {
                return null;
            }

            var info = new CommandData
            {
                TypeSymbol = typeSymbol,
                FullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                Namespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? "",
                ClassName = typeSymbol.Name,
                InferredName = InferCommandName(typeSymbol.Name),
                CommandAttribute = ExtractCommandAttributeData(commandAttr)
            };

            // Extract options
            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IPropertySymbol property)
                {
                    ExtractPropertyMetadata(property, info);
                }
            }

            // Extract subcommands and type-level options from attributes
            foreach (var attr in typeSymbol.GetAttributes())
            {
                var attrName = attr.AttributeClass?.ToDisplayString();
                if (attrName == SubcommandAttributeFullName)
                {
                    ExtractSubcommandMetadata(attr, info);
                }
                else if (attrName == HelpOptionAttributeFullName && info.HelpOption == null)
                {
                    info.HelpOption = ExtractTypeLevelHelpOptionData(attr);
                }
                else if (attrName == VersionOptionAttributeFullName && info.VersionOption == null)
                {
                    info.VersionOption = ExtractTypeLevelVersionOptionData(attr);
                }
                else if (attrName == VersionOptionFromMemberAttributeFullName && info.VersionOption == null)
                {
                    info.VersionOption = ExtractVersionOptionFromMemberData(attr);
                }
            }

            // Check for OnExecute/OnExecuteAsync methods
            ExtractExecuteMethods(typeSymbol, info);

            // Extract special properties (Parent, Subcommand, RemainingArguments)
            ExtractSpecialProperties(typeSymbol, info);

            // Extract constructor info for dependency injection
            ExtractConstructors(typeSymbol, info);

            return info;
        }

        private static void ExtractSpecialProperties(INamedTypeSymbol typeSymbol, CommandData info)
        {
            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is not IPropertySymbol property)
                    continue;

                // Check for Parent property
                if (property.Name == "Parent" && property.SetMethod != null)
                {
                    info.SpecialProperties.ParentPropertyName = property.Name;
                    info.SpecialProperties.ParentPropertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }

                // Check for Subcommand property
                if (property.Name == "Subcommand" && property.SetMethod != null)
                {
                    info.SpecialProperties.SubcommandPropertyName = property.Name;
                    info.SpecialProperties.SubcommandPropertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }

                // Check for RemainingArguments or RemainingArgs property
                if ((property.Name == "RemainingArguments" || property.Name == "RemainingArgs") && property.SetMethod != null)
                {
                    var typeStr = property.Type.ToDisplayString();
                    // Must be string[] or IReadOnlyList<string> or compatible
                    // Handle nullable annotations (string?[], string[]?, etc.)
                    if (typeStr.Contains("string[]") ||
                        typeStr.Contains("String[]") ||
                        typeStr.Contains("IReadOnlyList<string>") ||
                        typeStr.Contains("List<string>") ||
                        typeStr.Contains("IEnumerable<string>") ||
                        (property.Type is IArrayTypeSymbol arrayType && arrayType.ElementType.SpecialType == SpecialType.System_String))
                    {
                        info.SpecialProperties.RemainingArgumentsPropertyName = property.Name;
                        info.SpecialProperties.RemainingArgumentsPropertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        // Track whether this is an array type (needs special handling for conversion from IReadOnlyList)
                        info.SpecialProperties.RemainingArgumentsIsArray = property.Type is IArrayTypeSymbol;
                    }
                }
            }
        }

        private static void ExtractConstructors(INamedTypeSymbol typeSymbol, CommandData info)
        {
            // Get all public instance constructors, ordered by parameter count descending
            var constructors = typeSymbol.InstanceConstructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                .OrderByDescending(c => c.Parameters.Length)
                .ToArray();

            foreach (var ctor in constructors)
            {
                var ctorData = new ConstructorData();
                foreach (var param in ctor.Parameters)
                {
                    ctorData.Parameters.Add(new ConstructorParameterData
                    {
                        TypeName = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Name = param.Name
                    });
                }
                info.Constructors.Add(ctorData);
            }
        }

        private static CommandAttributeData ExtractCommandAttributeData(AttributeData attr)
        {
            var data = new CommandAttributeData();

            // Get constructor arguments (Name, additional names)
            if (attr.ConstructorArguments.Length > 0)
            {
                var firstArg = attr.ConstructorArguments[0];
                if (firstArg.Kind == TypedConstantKind.Array)
                {
                    var names = firstArg.Values.Select(v => v.Value?.ToString()).Where(n => n != null).ToArray();
                    if (names.Length > 0)
                    {
                        data.Name = names[0];
                        data.AdditionalNames = names.Skip(1).ToArray()!;
                    }
                }
                else if (firstArg.Value is string name)
                {
                    data.Name = name;
                }
            }

            // Get named arguments
            foreach (var arg in attr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "Name":
                        data.Name = arg.Value.Value?.ToString();
                        break;
                    case "Description":
                        data.Description = arg.Value.Value?.ToString();
                        break;
                    case "FullName":
                        data.FullName = arg.Value.Value?.ToString();
                        break;
                    case "ExtendedHelpText":
                        data.ExtendedHelpText = arg.Value.Value?.ToString();
                        break;
                    case "ShowInHelpText":
                        data.ShowInHelpText = (bool?)arg.Value.Value;
                        break;
                    case "AllowArgumentSeparator":
                        data.AllowArgumentSeparator = (bool?)arg.Value.Value;
                        break;
                    case "ClusterOptions":
                        data.ClusterOptions = (bool?)arg.Value.Value;
                        break;
                    case "UsePagerForHelpText":
                        data.UsePagerForHelpText = (bool?)arg.Value.Value;
                        break;
                    case "ResponseFileHandling":
                        data.ResponseFileHandling = (int?)arg.Value.Value;
                        break;
                    case "OptionsComparison":
                        data.OptionsComparison = (int?)arg.Value.Value;
                        break;
                    case "UnrecognizedArgumentHandling":
                        data.UnrecognizedArgumentHandling = (int?)arg.Value.Value;
                        break;
                }
            }

            return data;
        }

        private static void ExtractPropertyMetadata(IPropertySymbol property, CommandData info)
        {
            foreach (var attr in property.GetAttributes())
            {
                var attrName = attr.AttributeClass?.ToDisplayString();

                switch (attrName)
                {
                    case OptionAttributeFullName:
                        info.Options.Add(ExtractOptionData(property, attr));
                        break;
                    case ArgumentAttributeFullName:
                        info.Arguments.Add(ExtractArgumentData(property, attr));
                        break;
                    case HelpOptionAttributeFullName:
                        info.HelpOption = ExtractHelpOptionData(property, attr);
                        break;
                    case VersionOptionAttributeFullName:
                        info.VersionOption = ExtractVersionOptionData(property, attr);
                        break;
                }
            }
        }

        private static OptionData ExtractOptionData(IPropertySymbol property, AttributeData attr)
        {
            var data = new OptionData
            {
                PropertyName = property.Name,
                PropertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            };

            // Template from constructor
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string template)
            {
                data.Template = template;
            }

            foreach (var arg in attr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "Template":
                        data.Template = arg.Value.Value?.ToString();
                        break;
                    case "ShortName":
                        data.ShortName = arg.Value.Value?.ToString();
                        break;
                    case "LongName":
                        data.LongName = arg.Value.Value?.ToString();
                        break;
                    case "SymbolName":
                        data.SymbolName = arg.Value.Value?.ToString();
                        break;
                    case "ValueName":
                        data.ValueName = arg.Value.Value?.ToString();
                        break;
                    case "Description":
                        data.Description = arg.Value.Value?.ToString();
                        break;
                    case "ShowInHelpText":
                        data.ShowInHelpText = (bool?)arg.Value.Value;
                        break;
                    case "Inherited":
                        data.Inherited = (bool?)arg.Value.Value;
                        break;
                    case "CommandOptionType":
                        data.OptionType = (int?)arg.Value.Value;
                        data.OptionTypeExplicitlySet = true;
                        break;
                }
            }

            // Infer OptionType from property type if not explicitly set
            data.InferredOptionType = InferOptionType(property.Type);

            // Extract validation attributes
            ExtractValidationAttributes(property, data.Validators);

            return data;
        }

        /// <summary>
        /// Infers CommandOptionType from property type.
        /// CommandOptionType enum values:
        ///   MultipleValue = 0
        ///   SingleValue = 1
        ///   SingleOrNoValue = 2
        ///   NoValue = 3
        /// </summary>
        private static int InferOptionType(ITypeSymbol type)
        {
            // Handle nullable types - unwrap to get underlying type
            if (type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                type = namedType.TypeArguments[0];
            }

            var typeFullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            // Boolean -> NoValue (flag)
            if (type.SpecialType == SpecialType.System_Boolean)
            {
                return 3; // NoValue
            }

            // Boolean array -> NoValue (counting flag)
            if (type is IArrayTypeSymbol arrayType && arrayType.ElementType.SpecialType == SpecialType.System_Boolean)
            {
                return 3; // NoValue
            }

            // ValueTuple<bool, T> or Nullable value type -> SingleOrNoValue
            if (type is INamedTypeSymbol namedType2)
            {
                // Check for ValueTuple<bool, T>
                if (namedType2.IsTupleType && namedType2.TupleElements.Length == 2)
                {
                    var first = namedType2.TupleElements[0];
                    if (first.Type.SpecialType == SpecialType.System_Boolean)
                    {
                        return 2; // SingleOrNoValue
                    }
                }
            }

            // Arrays and collections -> MultipleValue
            if (type is IArrayTypeSymbol)
            {
                return 0; // MultipleValue
            }

            // Check for common collection types
            if (type is INamedTypeSymbol collType)
            {
                var originalDef = collType.OriginalDefinition.ToDisplayString();
                if (originalDef.StartsWith("System.Collections.Generic.IEnumerable<") ||
                    originalDef.StartsWith("System.Collections.Generic.ICollection<") ||
                    originalDef.StartsWith("System.Collections.Generic.IList<") ||
                    originalDef.StartsWith("System.Collections.Generic.List<") ||
                    originalDef.StartsWith("System.Collections.Generic.HashSet<") ||
                    originalDef == "System.Collections.IEnumerable" ||
                    originalDef == "System.Collections.ICollection" ||
                    originalDef == "System.Collections.IList")
                {
                    // String is IEnumerable but should be SingleValue
                    if (type.SpecialType != SpecialType.System_String)
                    {
                        return 0; // MultipleValue
                    }
                }
            }

            // Default: SingleValue
            return 1; // SingleValue
        }

        private static ArgumentData ExtractArgumentData(IPropertySymbol property, AttributeData attr)
        {
            var data = new ArgumentData
            {
                PropertyName = property.Name,
                PropertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            };

            // Order from constructor
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is int order)
            {
                data.Order = order;
            }

            foreach (var arg in attr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "Name":
                        data.Name = arg.Value.Value?.ToString();
                        break;
                    case "Description":
                        data.Description = arg.Value.Value?.ToString();
                        break;
                    case "ShowInHelpText":
                        data.ShowInHelpText = (bool?)arg.Value.Value;
                        break;
                }
            }

            // Extract validation attributes
            ExtractValidationAttributes(property, data.Validators);

            return data;
        }

        private static HelpOptionData ExtractHelpOptionData(IPropertySymbol property, AttributeData attr)
        {
            var data = new HelpOptionData
            {
                PropertyName = property.Name
            };

            // Template from constructor
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string template)
            {
                data.Template = template;
            }

            foreach (var arg in attr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "Template":
                        data.Template = arg.Value.Value?.ToString();
                        break;
                    case "Description":
                        data.Description = arg.Value.Value?.ToString();
                        break;
                    case "Inherited":
                        data.Inherited = (bool?)arg.Value.Value;
                        break;
                }
            }

            return data;
        }

        private static VersionOptionData ExtractVersionOptionData(IPropertySymbol property, AttributeData attr)
        {
            var data = new VersionOptionData
            {
                PropertyName = property.Name
            };

            // Template from constructor
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string template)
            {
                data.Template = template;
            }

            foreach (var arg in attr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "Template":
                        data.Template = arg.Value.Value?.ToString();
                        break;
                    case "Version":
                        data.Version = arg.Value.Value?.ToString();
                        break;
                    case "Description":
                        data.Description = arg.Value.Value?.ToString();
                        break;
                }
            }

            return data;
        }

        private static HelpOptionData ExtractTypeLevelHelpOptionData(AttributeData attr)
        {
            var data = new HelpOptionData();

            // Template from constructor
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string template)
            {
                data.Template = template;
            }

            foreach (var arg in attr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "Template":
                        data.Template = arg.Value.Value?.ToString();
                        break;
                    case "Description":
                        data.Description = arg.Value.Value?.ToString();
                        break;
                    case "Inherited":
                        data.Inherited = (bool?)arg.Value.Value;
                        break;
                }
            }

            return data;
        }

        private static VersionOptionData ExtractVersionOptionFromMemberData(AttributeData attr)
        {
            var data = new VersionOptionData();

            // Template from constructor (optional first argument)
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string template)
            {
                data.Template = template;
            }

            foreach (var arg in attr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "Template":
                        data.Template = arg.Value.Value?.ToString();
                        break;
                    case "MemberName":
                        data.MemberName = arg.Value.Value?.ToString();
                        break;
                    case "Description":
                        data.Description = arg.Value.Value?.ToString();
                        break;
                }
            }

            return data;
        }

        private static VersionOptionData ExtractTypeLevelVersionOptionData(AttributeData attr)
        {
            var data = new VersionOptionData();

            // Version from first constructor argument (single-arg constructor)
            if (attr.ConstructorArguments.Length == 1 && attr.ConstructorArguments[0].Value is string version)
            {
                data.Version = version;
            }
            // Two-arg constructor: (template, version)
            else if (attr.ConstructorArguments.Length >= 2)
            {
                if (attr.ConstructorArguments[0].Value is string tmpl)
                    data.Template = tmpl;
                if (attr.ConstructorArguments[1].Value is string ver)
                    data.Version = ver;
            }

            foreach (var arg in attr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "Template":
                        data.Template = arg.Value.Value?.ToString();
                        break;
                    case "Version":
                        data.Version = arg.Value.Value?.ToString();
                        break;
                    case "Description":
                        data.Description = arg.Value.Value?.ToString();
                        break;
                }
            }

            return data;
        }

        /// <summary>
        /// Extracts validation attributes (e.g., [Required], [Range], [StringLength]) from a property.
        /// </summary>
        private static void ExtractValidationAttributes(IPropertySymbol property, List<ValidationAttributeData> validators)
        {
            foreach (var attr in property.GetAttributes())
            {
                var attrClass = attr.AttributeClass;
                if (attrClass == null) continue;

                // Check if attribute inherits from ValidationAttribute
                if (!InheritsFromValidationAttribute(attrClass)) continue;

                var validatorData = new ValidationAttributeData
                {
                    TypeName = attrClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                };

                // Extract constructor arguments
                foreach (var ctorArg in attr.ConstructorArguments)
                {
                    validatorData.ConstructorArguments.Add(FormatTypedConstant(ctorArg));
                }

                // Extract named arguments
                foreach (var namedArg in attr.NamedArguments)
                {
                    validatorData.NamedArguments[namedArg.Key] = FormatTypedConstant(namedArg.Value);
                }

                validators.Add(validatorData);
            }
        }

        private static bool InheritsFromValidationAttribute(INamedTypeSymbol? typeSymbol)
        {
            while (typeSymbol != null)
            {
                if (typeSymbol.ToDisplayString() == "System.ComponentModel.DataAnnotations.ValidationAttribute")
                {
                    return true;
                }
                typeSymbol = typeSymbol.BaseType;
            }
            return false;
        }

        private static string FormatTypedConstant(TypedConstant constant)
        {
            if (constant.IsNull)
            {
                return "null";
            }

            switch (constant.Kind)
            {
                case TypedConstantKind.Primitive:
                    if (constant.Value is string s)
                    {
                        return $"\"{EscapeString(s)}\"";
                    }
                    if (constant.Value is bool b)
                    {
                        return b ? "true" : "false";
                    }
                    if (constant.Value is char c)
                    {
                        return $"'{c}'";
                    }
                    return constant.Value?.ToString() ?? "null";

                case TypedConstantKind.Enum:
                    return $"({constant.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}){constant.Value}";

                case TypedConstantKind.Type:
                    return $"typeof({((INamedTypeSymbol)constant.Value!).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})";

                case TypedConstantKind.Array:
                    var elements = constant.Values.Select(v => FormatTypedConstant(v));
                    return $"new[] {{ {string.Join(", ", elements)} }}";

                default:
                    return constant.Value?.ToString() ?? "null";
            }
        }

        private static void ExtractSubcommandMetadata(AttributeData attr, CommandData info)
        {
            if (attr.ConstructorArguments.Length > 0)
            {
                var types = attr.ConstructorArguments[0];
                if (types.Kind == TypedConstantKind.Array)
                {
                    foreach (var typeArg in types.Values)
                    {
                        if (typeArg.Value is INamedTypeSymbol subType)
                        {
                            info.Subcommands.Add(new SubcommandData
                            {
                                TypeName = subType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                            });
                        }
                    }
                }
                else if (types.Value is INamedTypeSymbol singleType)
                {
                    info.Subcommands.Add(new SubcommandData
                    {
                        TypeName = singleType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    });
                }
            }
        }

        private static void ExtractExecuteMethods(INamedTypeSymbol typeSymbol, CommandData info)
        {
            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IMethodSymbol method)
                {
                    if (method.Name == "OnExecute")
                    {
                        info.HasOnExecute = true;
                        info.OnExecuteIsAsync = false;
                        AnalyzeExecuteMethod(method, info);
                    }
                    else if (method.Name == "OnExecuteAsync")
                    {
                        info.HasOnExecute = true;
                        info.OnExecuteIsAsync = true;
                        AnalyzeExecuteMethod(method, info);
                    }
                    else if (method.Name == "OnValidate")
                    {
                        info.HasOnValidate = true;
                    }
                    else if (method.Name == "OnValidationError")
                    {
                        info.HasOnValidationError = true;
                    }
                }
            }
        }

        private static void AnalyzeExecuteMethod(IMethodSymbol method, CommandData info)
        {
            // Check return type
            if (method.ReturnType.SpecialType == SpecialType.System_Int32)
            {
                info.OnExecuteReturnsInt = true;
            }
            else if (method.ReturnType is INamedTypeSymbol namedType)
            {
                // Check for Task<int>
                if (namedType.Name == "Task" && namedType.TypeArguments.Length == 1 &&
                    namedType.TypeArguments[0].SpecialType == SpecialType.System_Int32)
                {
                    info.OnExecuteReturnsInt = true;
                }
            }

            // Check parameters
            foreach (var param in method.Parameters)
            {
                var typeName = param.Type.ToDisplayString();
                if (typeName.Contains("CommandLineApplication"))
                {
                    info.OnExecuteHasAppParameter = true;
                }
                else if (typeName.Contains("CancellationToken"))
                {
                    info.OnExecuteHasCancellationToken = true;
                }
            }
        }

        private static string GenerateMetadataProvider(CommandData info)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using McMaster.Extensions.CommandLineUtils;");
            sb.AppendLine("using McMaster.Extensions.CommandLineUtils.Abstractions;");
            sb.AppendLine("using McMaster.Extensions.CommandLineUtils.SourceGeneration;");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(info.Namespace))
            {
                sb.AppendLine($"namespace {info.Namespace}");
                sb.AppendLine("{");
            }

            var indent = string.IsNullOrEmpty(info.Namespace) ? "" : "    ";

            // Generate the metadata provider class
            sb.AppendLine($"{indent}internal sealed class {info.ClassName}__GeneratedMetadataProvider : ICommandMetadataProvider<{info.ClassName}>");
            sb.AppendLine($"{indent}{{");

            // Singleton instance
            sb.AppendLine($"{indent}    public static readonly {info.ClassName}__GeneratedMetadataProvider Instance = new();");
            sb.AppendLine();

            // ModelType
            sb.AppendLine($"{indent}    public Type ModelType => typeof({info.ClassName});");
            sb.AppendLine();

            // Options
            GenerateOptionsProperty(sb, info, indent);

            // Arguments
            GenerateArgumentsProperty(sb, info, indent);

            // Subcommands
            GenerateSubcommandsProperty(sb, info, indent);

            // CommandData
            GenerateCommandDataProperty(sb, info, indent);

            // ExecuteHandler
            GenerateExecuteHandler(sb, info, indent);

            // ValidateHandler
            GenerateValidateHandler(sb, info, indent);

            // ValidationErrorHandler
            GenerateValidationErrorHandler(sb, info, indent);

            // SpecialProperties
            GenerateSpecialPropertiesProperty(sb, info, indent);

            // HelpOption
            GenerateHelpOptionProperty(sb, info, indent);

            // VersionOption
            GenerateVersionOptionProperty(sb, info, indent);

            // GetModelFactory
            sb.AppendLine($"{indent}    public IModelFactory<{info.ClassName}> GetModelFactory(IServiceProvider? services)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        return new GeneratedModelFactory(services);");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    IModelFactory ICommandMetadataProvider.GetModelFactory(IServiceProvider? services)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        return GetModelFactory(services);");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            // Nested model factory
            GenerateModelFactory(sb, info, indent);

            sb.AppendLine($"{indent}}}");

            if (!string.IsNullOrEmpty(info.Namespace))
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private static void GenerateOptionsProperty(StringBuilder sb, CommandData info, string indent)
        {
            if (info.Options.Count == 0)
            {
                sb.AppendLine($"{indent}    public IReadOnlyList<OptionMetadata> Options => Array.Empty<OptionMetadata>();");
            }
            else
            {
                sb.AppendLine($"{indent}    private static readonly IReadOnlyList<OptionMetadata> _options = new OptionMetadata[]");
                sb.AppendLine($"{indent}    {{");
                foreach (var opt in info.Options)
                {
                    sb.AppendLine($"{indent}        new OptionMetadata(");
                    sb.AppendLine($"{indent}            propertyName: \"{opt.PropertyName}\",");
                    sb.AppendLine($"{indent}            propertyType: typeof({opt.PropertyType}),");
                    sb.AppendLine($"{indent}            getter: static obj => (({info.ClassName})obj).{opt.PropertyName},");
                    sb.AppendLine($"{indent}            setter: static (obj, val) => (({info.ClassName})obj).{opt.PropertyName} = ({opt.PropertyType})val!)");
                    sb.AppendLine($"{indent}        {{");
                    if (opt.Template != null)
                        sb.AppendLine($"{indent}            Template = \"{EscapeString(opt.Template)}\",");
                    if (opt.ShortName != null)
                        sb.AppendLine($"{indent}            ShortName = \"{EscapeString(opt.ShortName)}\",");
                    if (opt.LongName != null)
                        sb.AppendLine($"{indent}            LongName = \"{EscapeString(opt.LongName)}\",");
                    if (opt.Description != null)
                        sb.AppendLine($"{indent}            Description = \"{EscapeString(opt.Description)}\",");
                    if (opt.ShowInHelpText.HasValue)
                        sb.AppendLine($"{indent}            ShowInHelpText = {opt.ShowInHelpText.Value.ToString().ToLowerInvariant()},");
                    if (opt.Inherited.HasValue)
                        sb.AppendLine($"{indent}            Inherited = {opt.Inherited.Value.ToString().ToLowerInvariant()},");
                    // Always emit OptionType and OptionTypeExplicitlySet
                    var optionType = opt.OptionType ?? opt.InferredOptionType;
                    sb.AppendLine($"{indent}            OptionType = (CommandOptionType){optionType},");
                    sb.AppendLine($"{indent}            OptionTypeExplicitlySet = {opt.OptionTypeExplicitlySet.ToString().ToLowerInvariant()},");
                    // Emit validators if any
                    if (opt.Validators.Count > 0)
                    {
                        GenerateValidatorsProperty(sb, opt.Validators, indent + "            ");
                    }
                    sb.AppendLine($"{indent}        }},");
                }
                sb.AppendLine($"{indent}    }};");
                sb.AppendLine();
                sb.AppendLine($"{indent}    public IReadOnlyList<OptionMetadata> Options => _options;");
            }
            sb.AppendLine();
        }

        private static void GenerateArgumentsProperty(StringBuilder sb, CommandData info, string indent)
        {
            if (info.Arguments.Count == 0)
            {
                sb.AppendLine($"{indent}    public IReadOnlyList<ArgumentMetadata> Arguments => Array.Empty<ArgumentMetadata>();");
            }
            else
            {
                sb.AppendLine($"{indent}    private static readonly IReadOnlyList<ArgumentMetadata> _arguments = new ArgumentMetadata[]");
                sb.AppendLine($"{indent}    {{");
                foreach (var arg in info.Arguments.OrderBy(a => a.Order))
                {
                    sb.AppendLine($"{indent}        new ArgumentMetadata(");
                    sb.AppendLine($"{indent}            propertyName: \"{arg.PropertyName}\",");
                    sb.AppendLine($"{indent}            propertyType: typeof({arg.PropertyType}),");
                    sb.AppendLine($"{indent}            order: {arg.Order},");
                    sb.AppendLine($"{indent}            getter: static obj => (({info.ClassName})obj).{arg.PropertyName},");
                    sb.AppendLine($"{indent}            setter: static (obj, val) => (({info.ClassName})obj).{arg.PropertyName} = ({arg.PropertyType})val!)");
                    sb.AppendLine($"{indent}        {{");
                    if (arg.Name != null)
                        sb.AppendLine($"{indent}            Name = \"{EscapeString(arg.Name)}\",");
                    if (arg.Description != null)
                        sb.AppendLine($"{indent}            Description = \"{EscapeString(arg.Description)}\",");
                    if (arg.ShowInHelpText.HasValue)
                        sb.AppendLine($"{indent}            ShowInHelpText = {arg.ShowInHelpText.Value.ToString().ToLowerInvariant()},");
                    // Emit validators if any
                    if (arg.Validators.Count > 0)
                    {
                        GenerateValidatorsProperty(sb, arg.Validators, indent + "            ");
                    }
                    sb.AppendLine($"{indent}        }},");
                }
                sb.AppendLine($"{indent}    }};");
                sb.AppendLine();
                sb.AppendLine($"{indent}    public IReadOnlyList<ArgumentMetadata> Arguments => _arguments;");
            }
            sb.AppendLine();
        }

        private static void GenerateValidatorsProperty(StringBuilder sb, List<ValidationAttributeData> validators, string indent)
        {
            sb.AppendLine($"{indent}Validators = new ValidationAttribute[]");
            sb.AppendLine($"{indent}{{");
            foreach (var validator in validators)
            {
                var ctorArgs = string.Join(", ", validator.ConstructorArguments);
                sb.Append($"{indent}    new {validator.TypeName}({ctorArgs})");

                if (validator.NamedArguments.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"{indent}    {{");
                    foreach (var namedArg in validator.NamedArguments)
                    {
                        sb.AppendLine($"{indent}        {namedArg.Key} = {namedArg.Value},");
                    }
                    sb.Append($"{indent}    }}");
                }
                sb.AppendLine(",");
            }
            sb.AppendLine($"{indent}}},");
        }

        private static void GenerateSubcommandsProperty(StringBuilder sb, CommandData info, string indent)
        {
            if (info.Subcommands.Count == 0)
            {
                sb.AppendLine($"{indent}    public IReadOnlyList<SubcommandMetadata> Subcommands => Array.Empty<SubcommandMetadata>();");
            }
            else
            {
                sb.AppendLine($"{indent}    private static readonly IReadOnlyList<SubcommandMetadata> _subcommands = new SubcommandMetadata[]");
                sb.AppendLine($"{indent}    {{");
                foreach (var sub in info.Subcommands)
                {
                    sb.AppendLine($"{indent}        new SubcommandMetadata(typeof({sub.TypeName}))");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            MetadataProviderFactory = () => CommandMetadataRegistry.TryGetProvider(typeof({sub.TypeName}), out var p) ? p : DefaultMetadataResolver.Instance.GetProvider(typeof({sub.TypeName}))");
                    sb.AppendLine($"{indent}        }},");
                }
                sb.AppendLine($"{indent}    }};");
                sb.AppendLine();
                sb.AppendLine($"{indent}    public IReadOnlyList<SubcommandMetadata> Subcommands => _subcommands;");
            }
            sb.AppendLine();
        }

        private static void GenerateCommandDataProperty(StringBuilder sb, CommandData info, string indent)
        {
            var cmd = info.CommandAttribute;
            sb.AppendLine($"{indent}    public CommandMetadata? CommandInfo => new CommandMetadata");
            sb.AppendLine($"{indent}    {{");
            // Use explicit name if set, otherwise use inferred name from class name
            var commandName = cmd.Name ?? info.InferredName;
            sb.AppendLine($"{indent}        Name = \"{EscapeString(commandName)}\",");
            if (cmd.AdditionalNames?.Length > 0)
                sb.AppendLine($"{indent}        AdditionalNames = new[] {{ {string.Join(", ", cmd.AdditionalNames.Select(n => $"\"{EscapeString(n)}\""))} }},");
            if (cmd.Description != null)
                sb.AppendLine($"{indent}        Description = \"{EscapeString(cmd.Description)}\",");
            if (cmd.FullName != null)
                sb.AppendLine($"{indent}        FullName = \"{EscapeString(cmd.FullName)}\",");
            if (cmd.ExtendedHelpText != null)
                sb.AppendLine($"{indent}        ExtendedHelpText = \"{EscapeString(cmd.ExtendedHelpText)}\",");
            if (cmd.ShowInHelpText.HasValue)
                sb.AppendLine($"{indent}        ShowInHelpText = {cmd.ShowInHelpText.Value.ToString().ToLowerInvariant()},");
            if (cmd.AllowArgumentSeparator.HasValue)
                sb.AppendLine($"{indent}        AllowArgumentSeparator = {cmd.AllowArgumentSeparator.Value.ToString().ToLowerInvariant()},");
            if (cmd.ClusterOptions.HasValue)
                sb.AppendLine($"{indent}        ClusterOptions = {cmd.ClusterOptions.Value.ToString().ToLowerInvariant()},");
            if (cmd.UsePagerForHelpText.HasValue)
                sb.AppendLine($"{indent}        UsePagerForHelpText = {cmd.UsePagerForHelpText.Value.ToString().ToLowerInvariant()},");
            if (cmd.ResponseFileHandling.HasValue)
                sb.AppendLine($"{indent}        ResponseFileHandling = (ResponseFileHandling){cmd.ResponseFileHandling.Value},");
            if (cmd.OptionsComparison.HasValue)
                sb.AppendLine($"{indent}        OptionsComparison = (StringComparison){cmd.OptionsComparison.Value},");
            if (cmd.UnrecognizedArgumentHandling.HasValue)
                sb.AppendLine($"{indent}        UnrecognizedArgumentHandling = (UnrecognizedArgumentHandling){cmd.UnrecognizedArgumentHandling.Value},");
            sb.AppendLine($"{indent}    }};");
            sb.AppendLine();
        }

        private static void GenerateExecuteHandler(StringBuilder sb, CommandData info, string indent)
        {
            if (!info.HasOnExecute)
            {
                sb.AppendLine($"{indent}    public IExecuteHandler? ExecuteHandler => null;");
            }
            else
            {
                sb.AppendLine($"{indent}    public IExecuteHandler? ExecuteHandler => GeneratedExecuteHandler.Instance;");
                sb.AppendLine();
                sb.AppendLine($"{indent}    private sealed class GeneratedExecuteHandler : IExecuteHandler");
                sb.AppendLine($"{indent}    {{");
                sb.AppendLine($"{indent}        public static readonly GeneratedExecuteHandler Instance = new();");
                sb.AppendLine();
                sb.AppendLine($"{indent}        public bool IsAsync => {(info.OnExecuteIsAsync ? "true" : "false")};");
                sb.AppendLine();

                // Build the argument list based on what parameters the method has
                var args = new List<string>();
                if (info.OnExecuteHasAppParameter)
                {
                    args.Add("app");
                }
                if (info.OnExecuteHasCancellationToken)
                {
                    args.Add("cancellationToken");
                }
                var argsString = string.Join(", ", args);

                if (info.OnExecuteIsAsync)
                {
                    // OnExecuteAsync method - use async/await
                    sb.AppendLine($"{indent}        public async Task<int> InvokeAsync(object model, CommandLineApplication app, CancellationToken cancellationToken)");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            var typedModel = ({info.ClassName})model;");
                    if (info.OnExecuteReturnsInt)
                    {
                        sb.AppendLine($"{indent}            return await typedModel.OnExecuteAsync({argsString});");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}            await typedModel.OnExecuteAsync({argsString});");
                        sb.AppendLine($"{indent}            return 0;");
                    }
                }
                else
                {
                    // OnExecute method - use Task.FromResult to avoid async warnings
                    sb.AppendLine($"{indent}        public Task<int> InvokeAsync(object model, CommandLineApplication app, CancellationToken cancellationToken)");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            var typedModel = ({info.ClassName})model;");
                    if (info.OnExecuteReturnsInt)
                    {
                        sb.AppendLine($"{indent}            return Task.FromResult(typedModel.OnExecute({argsString}));");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}            typedModel.OnExecute({argsString});");
                        sb.AppendLine($"{indent}            return Task.FromResult(0);");
                    }
                }

                sb.AppendLine($"{indent}        }}");
                sb.AppendLine($"{indent}    }}");
            }
            sb.AppendLine();
        }

        private static void GenerateValidateHandler(StringBuilder sb, CommandData info, string indent)
        {
            if (!info.HasOnValidate)
            {
                sb.AppendLine($"{indent}    public IValidateHandler? ValidateHandler => null;");
            }
            else
            {
                sb.AppendLine($"{indent}    public IValidateHandler? ValidateHandler => GeneratedValidateHandler.Instance;");
                sb.AppendLine();
                sb.AppendLine($"{indent}    private sealed class GeneratedValidateHandler : IValidateHandler");
                sb.AppendLine($"{indent}    {{");
                sb.AppendLine($"{indent}        public static readonly GeneratedValidateHandler Instance = new();");
                sb.AppendLine();
                sb.AppendLine($"{indent}        public ValidationResult Invoke(object model, ValidationContext context)");
                sb.AppendLine($"{indent}        {{");
                sb.AppendLine($"{indent}            var typedModel = ({info.ClassName})model;");
                sb.AppendLine($"{indent}            return typedModel.OnValidate(context);");
                sb.AppendLine($"{indent}        }}");
                sb.AppendLine($"{indent}    }}");
            }
            sb.AppendLine();
        }

        private static void GenerateValidationErrorHandler(StringBuilder sb, CommandData info, string indent)
        {
            if (!info.HasOnValidationError)
            {
                sb.AppendLine($"{indent}    public IValidationErrorHandler? ValidationErrorHandler => null;");
            }
            else
            {
                sb.AppendLine($"{indent}    public IValidationErrorHandler? ValidationErrorHandler => GeneratedValidationErrorHandler.Instance;");
                sb.AppendLine();
                sb.AppendLine($"{indent}    private sealed class GeneratedValidationErrorHandler : IValidationErrorHandler");
                sb.AppendLine($"{indent}    {{");
                sb.AppendLine($"{indent}        public static readonly GeneratedValidationErrorHandler Instance = new();");
                sb.AppendLine();
                sb.AppendLine($"{indent}        public int Invoke(object model, ValidationResult result)");
                sb.AppendLine($"{indent}        {{");
                sb.AppendLine($"{indent}            var typedModel = ({info.ClassName})model;");
                sb.AppendLine($"{indent}            return typedModel.OnValidationError(result);");
                sb.AppendLine($"{indent}        }}");
                sb.AppendLine($"{indent}    }}");
            }
            sb.AppendLine();
        }

        private static void GenerateSpecialPropertiesProperty(StringBuilder sb, CommandData info, string indent)
        {
            var sp = info.SpecialProperties;
            if (!sp.HasAny)
            {
                sb.AppendLine($"{indent}    public SpecialPropertiesMetadata? SpecialProperties => null;");
            }
            else
            {
                sb.AppendLine($"{indent}    public SpecialPropertiesMetadata? SpecialProperties => new SpecialPropertiesMetadata");
                sb.AppendLine($"{indent}    {{");

                if (sp.ParentPropertyName != null)
                {
                    sb.AppendLine($"{indent}        ParentSetter = static (obj, val) => (({info.ClassName})obj).{sp.ParentPropertyName} = ({sp.ParentPropertyType})val!,");
                    sb.AppendLine($"{indent}        ParentType = typeof({sp.ParentPropertyType}),");
                }

                if (sp.SubcommandPropertyName != null)
                {
                    sb.AppendLine($"{indent}        SubcommandSetter = static (obj, val) => (({info.ClassName})obj).{sp.SubcommandPropertyName} = ({sp.SubcommandPropertyType})val!,");
                    sb.AppendLine($"{indent}        SubcommandType = typeof({sp.SubcommandPropertyType}),");
                }

                if (sp.RemainingArgumentsPropertyName != null)
                {
                    // Handle string[] vs IReadOnlyList<string> based on actual type analysis (not string comparison)
                    // Array types need conversion from IReadOnlyList, collection types can be cast directly
                    if (sp.RemainingArgumentsIsArray)
                    {
                        sb.AppendLine($"{indent}        RemainingArgumentsSetter = static (obj, val) => (({info.ClassName})obj).{sp.RemainingArgumentsPropertyName} = val is string[] arr ? arr : ((System.Collections.Generic.IReadOnlyList<string>)val!).ToArray(),");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}        RemainingArgumentsSetter = static (obj, val) => (({info.ClassName})obj).{sp.RemainingArgumentsPropertyName} = ({sp.RemainingArgumentsPropertyType})val!,");
                    }
                    sb.AppendLine($"{indent}        RemainingArgumentsType = typeof({sp.RemainingArgumentsPropertyType}),");
                }

                sb.AppendLine($"{indent}    }};");
            }
            sb.AppendLine();
        }

        private static void GenerateHelpOptionProperty(StringBuilder sb, CommandData info, string indent)
        {
            if (info.HelpOption == null)
            {
                sb.AppendLine($"{indent}    public HelpOptionMetadata? HelpOption => null;");
            }
            else
            {
                sb.AppendLine($"{indent}    public HelpOptionMetadata? HelpOption => new HelpOptionMetadata");
                sb.AppendLine($"{indent}    {{");
                if (info.HelpOption.Template != null)
                    sb.AppendLine($"{indent}        Template = \"{EscapeString(info.HelpOption.Template)}\",");
                if (info.HelpOption.Description != null)
                    sb.AppendLine($"{indent}        Description = \"{EscapeString(info.HelpOption.Description)}\",");
                if (info.HelpOption.Inherited.HasValue)
                    sb.AppendLine($"{indent}        Inherited = {info.HelpOption.Inherited.Value.ToString().ToLowerInvariant()},");
                sb.AppendLine($"{indent}    }};");
            }
            sb.AppendLine();
        }

        private static void GenerateVersionOptionProperty(StringBuilder sb, CommandData info, string indent)
        {
            if (info.VersionOption == null)
            {
                sb.AppendLine($"{indent}    public VersionOptionMetadata? VersionOption => null;");
            }
            else
            {
                sb.AppendLine($"{indent}    public VersionOptionMetadata? VersionOption => new VersionOptionMetadata");
                sb.AppendLine($"{indent}    {{");
                if (info.VersionOption.Template != null)
                    sb.AppendLine($"{indent}        Template = \"{EscapeString(info.VersionOption.Template)}\",");
                if (info.VersionOption.Version != null)
                    sb.AppendLine($"{indent}        Version = \"{EscapeString(info.VersionOption.Version)}\",");
                if (info.VersionOption.Description != null)
                    sb.AppendLine($"{indent}        Description = \"{EscapeString(info.VersionOption.Description)}\",");
                if (info.VersionOption.MemberName != null)
                    sb.AppendLine($"{indent}        VersionGetter = static (obj) => (({info.ClassName})obj).{info.VersionOption.MemberName},");
                sb.AppendLine($"{indent}    }};");
            }
            sb.AppendLine();
        }

        private static void GenerateModelFactory(StringBuilder sb, CommandData info, string indent)
        {
            sb.AppendLine($"{indent}    private sealed class GeneratedModelFactory : IModelFactory<{info.ClassName}>");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        private readonly IServiceProvider? _services;");
            sb.AppendLine();
            sb.AppendLine($"{indent}        public GeneratedModelFactory(IServiceProvider? services) => _services = services;");
            sb.AppendLine();
            sb.AppendLine($"{indent}        public {info.ClassName} Create()");
            sb.AppendLine($"{indent}        {{");

            // First, try to get the model type directly from the service provider
            sb.AppendLine($"{indent}            if (_services != null)");
            sb.AppendLine($"{indent}            {{");
            sb.AppendLine($"{indent}                var instance = _services.GetService(typeof({info.ClassName})) as {info.ClassName};");
            sb.AppendLine($"{indent}                if (instance != null) return instance;");

            // Generate code for each constructor with parameters (ordered by parameter count descending)
            var constructorsWithParams = info.Constructors.Where(c => c.Parameters.Count > 0).ToList();
            if (constructorsWithParams.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"{indent}                // Try to create using constructor injection");

                for (int ctorIdx = 0; ctorIdx < constructorsWithParams.Count; ctorIdx++)
                {
                    var ctor = constructorsWithParams[ctorIdx];

                    // Generate variable declarations for each parameter
                    // Use intermediate 'service' variable to avoid 'as' operator issues with value types
                    for (int paramIdx = 0; paramIdx < ctor.Parameters.Count; paramIdx++)
                    {
                        var param = ctor.Parameters[paramIdx];
                        sb.AppendLine($"{indent}                var service{ctorIdx}_{paramIdx} = _services.GetService(typeof({param.TypeName}));");
                    }

                    // Check if all parameters were resolved (check service objects, not cast results)
                    var allParamsCheck = string.Join(" && ", Enumerable.Range(0, ctor.Parameters.Count).Select(i => $"service{ctorIdx}_{i} != null"));
                    sb.AppendLine($"{indent}                if ({allParamsCheck})");
                    sb.AppendLine($"{indent}                {{");

                    // Create instance with resolved parameters (cast to correct types)
                    var paramList = string.Join(", ", Enumerable.Range(0, ctor.Parameters.Count).Select(i => $"({ctor.Parameters[i].TypeName})service{ctorIdx}_{i}!"));
                    sb.AppendLine($"{indent}                    return new {info.ClassName}({paramList});");
                    sb.AppendLine($"{indent}                }}");
                }
            }

            sb.AppendLine($"{indent}            }}");

            // Check if there's a default constructor
            var hasDefaultConstructor = info.Constructors.Any(c => c.Parameters.Count == 0);
            if (hasDefaultConstructor)
            {
                sb.AppendLine($"{indent}            return new {info.ClassName}();");
            }
            else if (info.Constructors.Count > 0)
            {
                // No default constructor - provide specific error messages
                sb.AppendLine($"{indent}            if (_services == null)");
                sb.AppendLine($"{indent}            {{");
                sb.AppendLine($"{indent}                throw new InvalidOperationException(\"Unable to create instance of {info.ClassName}. The type requires constructor parameters but no IServiceProvider was configured. Use CommandLineApplication.Execute<T>(args, services) to provide services.\");");
                sb.AppendLine($"{indent}            }}");
                sb.AppendLine($"{indent}            throw new InvalidOperationException(\"Unable to create instance of {info.ClassName}. Required service(s) not registered in the IServiceProvider.\");");
            }
            else
            {
                // No public constructors at all - try anyway (will fail at compile time if not possible)
                sb.AppendLine($"{indent}            return new {info.ClassName}();");
            }

            sb.AppendLine($"{indent}        }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}        object IModelFactory.Create() => Create();");
            sb.AppendLine($"{indent}    }}");
        }

        private static string GenerateModuleInitializer(ImmutableArray<CommandData> commands)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using McMaster.Extensions.CommandLineUtils.SourceGeneration;");
            sb.AppendLine();
            sb.AppendLine("namespace McMaster.Extensions.CommandLineUtils.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    internal static class CommandMetadataRegistration");
            sb.AppendLine("    {");
            sb.AppendLine("        [ModuleInitializer]");
            sb.AppendLine("        internal static void RegisterAllProviders()");
            sb.AppendLine("        {");

            foreach (var cmd in commands)
            {
                var providerType = string.IsNullOrEmpty(cmd.Namespace)
                    ? $"global::{cmd.ClassName}__GeneratedMetadataProvider"
                    : $"global::{cmd.Namespace}.{cmd.ClassName}__GeneratedMetadataProvider";

                sb.AppendLine($"            CommandMetadataRegistry.Register(typeof(global::{cmd.FullTypeName}), {providerType}.Instance);");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string EscapeString(string s)
        {
            return s
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// Infers a command name from a type name by stripping the "Command" suffix
        /// and converting to kebab-case.
        /// </summary>
        private static string InferCommandName(string typeName)
        {
            const string cmd = "Command";
            if (typeName.Length > cmd.Length && typeName.EndsWith(cmd))
            {
                typeName = typeName.Substring(0, typeName.Length - cmd.Length);
            }

            return ToKebabCase(typeName);
        }

        /// <summary>
        /// Converts a PascalCase or camelCase string to kebab-case.
        /// </summary>
        private static string ToKebabCase(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var sb = new StringBuilder();
            var i = 0;
            var addDash = false;

            for (; i < str.Length; i++)
            {
                var ch = str[i];
                if (!char.IsLetterOrDigit(ch))
                {
                    continue;
                }

                addDash = !char.IsUpper(ch);
                sb.Append(char.ToLowerInvariant(ch));
                i++;
                break;
            }

            for (; i < str.Length; i++)
            {
                var ch = str[i];
                if (char.IsUpper(ch))
                {
                    if (addDash)
                    {
                        addDash = false;
                        sb.Append('-');
                    }

                    sb.Append(char.ToLowerInvariant(ch));
                }
                else if (char.IsLetterOrDigit(ch))
                {
                    addDash = true;
                    sb.Append(ch);
                }
                else
                {
                    addDash = false;
                    sb.Append('-');
                }
            }

            // trim trailing slashes
            while (sb.Length > 0 && sb[sb.Length - 1] == '-')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        // Marker attribute that can be used to opt-in to generation
        private const string AttributeSource = @"// <auto-generated/>
#nullable enable

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Marker attribute to indicate that source generation should create
    /// an ICommandMetadataProvider for this command. This attribute is optional
    /// when the CommandLineUtils.Generators package is referenced - all [Command]
    /// classes will automatically get generated providers.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class GenerateMetadataAttribute : System.Attribute
    {
    }
}
";
    }
}
