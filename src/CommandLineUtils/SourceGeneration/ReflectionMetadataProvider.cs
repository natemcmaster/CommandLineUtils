// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Provides command metadata by analyzing a type using reflection.
    /// This is the fallback when generated metadata is not available.
    /// </summary>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Uses reflection to analyze the model type")]
#elif NET472_OR_GREATER
#else
#error Target framework misconfiguration
#endif
    internal sealed class ReflectionMetadataProvider : ICommandMetadataProvider
    {
        private const BindingFlags MethodLookup = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private readonly Type _modelType;
        private readonly Lazy<IReadOnlyList<OptionMetadata>> _options;
        private readonly Lazy<IReadOnlyList<ArgumentMetadata>> _arguments;
        private readonly Lazy<IReadOnlyList<SubcommandMetadata>> _subcommands;
        private readonly Lazy<CommandMetadata?> _commandInfo;
        private readonly Lazy<IExecuteHandler?> _executeHandler;
        private readonly Lazy<IValidateHandler?> _validateHandler;
        private readonly Lazy<IValidationErrorHandler?> _validationErrorHandler;
        private readonly Lazy<SpecialPropertiesMetadata?> _specialProperties;
        private readonly Lazy<HelpOptionMetadata?> _helpOption;
        private readonly Lazy<VersionOptionMetadata?> _versionOption;

        /// <summary>
        /// Initializes a new instance of <see cref="ReflectionMetadataProvider"/>.
        /// </summary>
        /// <param name="modelType">The model type to analyze.</param>
        public ReflectionMetadataProvider(Type modelType)
        {
            _modelType = modelType ?? throw new ArgumentNullException(nameof(modelType));

            _options = new Lazy<IReadOnlyList<OptionMetadata>>(ExtractOptions);
            _arguments = new Lazy<IReadOnlyList<ArgumentMetadata>>(ExtractArguments);
            _subcommands = new Lazy<IReadOnlyList<SubcommandMetadata>>(ExtractSubcommands);
            _commandInfo = new Lazy<CommandMetadata?>(ExtractCommandInfo);
            _executeHandler = new Lazy<IExecuteHandler?>(ExtractExecuteHandler);
            _validateHandler = new Lazy<IValidateHandler?>(ExtractValidateHandler);
            _validationErrorHandler = new Lazy<IValidationErrorHandler?>(ExtractValidationErrorHandler);
            _specialProperties = new Lazy<SpecialPropertiesMetadata?>(ExtractSpecialProperties);
            _helpOption = new Lazy<HelpOptionMetadata?>(ExtractHelpOption);
            _versionOption = new Lazy<VersionOptionMetadata?>(ExtractVersionOption);
        }

        /// <inheritdoc />
        public Type ModelType => _modelType;

        /// <inheritdoc />
        public IReadOnlyList<OptionMetadata> Options => _options.Value;

        /// <inheritdoc />
        public IReadOnlyList<ArgumentMetadata> Arguments => _arguments.Value;

        /// <inheritdoc />
        public IReadOnlyList<SubcommandMetadata> Subcommands => _subcommands.Value;

        /// <inheritdoc />
        public CommandMetadata? CommandInfo => _commandInfo.Value;

        /// <inheritdoc />
        public IExecuteHandler? ExecuteHandler => _executeHandler.Value;

        /// <inheritdoc />
        public IValidateHandler? ValidateHandler => _validateHandler.Value;

        /// <inheritdoc />
        public IValidationErrorHandler? ValidationErrorHandler => _validationErrorHandler.Value;

        /// <inheritdoc />
        public SpecialPropertiesMetadata? SpecialProperties => _specialProperties.Value;

        /// <inheritdoc />
        public HelpOptionMetadata? HelpOption => _helpOption.Value;

        /// <inheritdoc />
        public VersionOptionMetadata? VersionOption => _versionOption.Value;

        /// <inheritdoc />
        public IModelFactory GetModelFactory(IServiceProvider? services)
        {
            return new ActivatorModelFactory(_modelType, services);
        }

        private IReadOnlyList<OptionMetadata> ExtractOptions()
        {
            var options = new List<OptionMetadata>();
            var props = ReflectionHelper.GetProperties(_modelType);

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<OptionAttribute>();
                if (attr == null)
                {
                    continue;
                }

                // Skip if it has HelpOption or VersionOption attribute (handled separately)
                if (prop.GetCustomAttribute<HelpOptionAttribute>() != null ||
                    prop.GetCustomAttribute<VersionOptionAttribute>() != null)
                {
                    continue;
                }

                // Check for conflicting Argument attribute
                if (prop.GetCustomAttribute<ArgumentAttribute>() != null)
                {
                    throw new InvalidOperationException(
                        Strings.BothOptionAndArgumentAttributesCannotBeSpecified(prop));
                }

                var getter = ReflectionHelper.GetPropertyGetter(prop);
                var setter = ReflectionHelper.GetPropertySetter(prop);
                var validators = prop.GetCustomAttributes<ValidationAttribute>().ToList();

                // Infer option names from property name if not specified in attribute
                string? template = attr.Template;
                string? shortName = attr.ShortName;
                string? longName = attr.LongName;
                string? valueName = attr.ValueName;

                // If no template specified, infer names from property name
                if (string.IsNullOrEmpty(template))
                {
                    // Infer longName from property name if not specified
                    // (empty string means "no long name", null means "infer from property")
                    if (longName == null)
                    {
                        longName = prop.Name.ToKebabCase();
                    }
                    // Infer shortName from longName if not specified
                    // (empty string means "no short name", null means "infer from long name")
                    if (shortName == null)
                    {
                        shortName = !string.IsNullOrEmpty(longName) ? longName.Substring(0, 1) : null;
                    }
                    // Infer valueName from property name if not specified
                    valueName ??= prop.Name.ToConstantCase();
                }

                // Use explicit OptionType if specified, otherwise try to infer from property type
                CommandOptionType optionType;
                if (attr.OptionType.HasValue)
                {
                    optionType = attr.OptionType.Value;
                }
                else if (!CommandOptionTypeMapper.Default.TryGetOptionType(prop.PropertyType, null!, out optionType))
                {
                    // For types that have custom parsers (like Uri), default to SingleValue
                    // This matches the behavior of the original OptionAttribute.GetOptionType()
                    // which would also require a custom parser for such types
                    optionType = CommandOptionType.SingleValue;
                }

                options.Add(new OptionMetadata(
                    propertyName: prop.Name,
                    propertyType: prop.PropertyType,
                    getter: obj => getter(obj),
                    setter: (obj, val) => setter(obj, val))
                {
                    Template = template,
                    ShortName = shortName,
                    LongName = longName,
                    SymbolName = attr.SymbolName,
                    ValueName = valueName,
                    Description = attr.Description,
                    OptionType = optionType,
                    OptionTypeExplicitlySet = attr.OptionType.HasValue,
                    ShowInHelpText = attr.ShowInHelpText,
                    Inherited = attr.Inherited,
                    Validators = validators,
                    DeclaringType = prop.DeclaringType
                });
            }

            return options;
        }

        private IReadOnlyList<ArgumentMetadata> ExtractArguments()
        {
            var arguments = new List<ArgumentMetadata>();
            var props = ReflectionHelper.GetProperties(_modelType);

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<ArgumentAttribute>();
                if (attr == null)
                {
                    continue;
                }

                var getter = ReflectionHelper.GetPropertyGetter(prop);
                var setter = ReflectionHelper.GetPropertySetter(prop);
                var validators = prop.GetCustomAttributes<ValidationAttribute>().ToList();

                var multipleValues = prop.PropertyType.IsArray ||
                    (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) &&
                     prop.PropertyType != typeof(string));

                arguments.Add(new ArgumentMetadata(
                    propertyName: prop.Name,
                    propertyType: prop.PropertyType,
                    order: attr.Order,
                    getter: obj => getter(obj),
                    setter: (obj, val) => setter(obj, val))
                {
                    Name = attr.Name,
                    Description = attr.Description,
                    ShowInHelpText = attr.ShowInHelpText,
                    MultipleValues = multipleValues,
                    Validators = validators,
                    DeclaringType = prop.DeclaringType
                });
            }

            // Sort by order
            return arguments.OrderBy(a => a.Order).ToList();
        }

        private IReadOnlyList<SubcommandMetadata> ExtractSubcommands()
        {
            var subcommands = new List<SubcommandMetadata>();
            var attributes = _modelType.GetCustomAttributes<SubcommandAttribute>();

            foreach (var attr in attributes)
            {
                foreach (var type in attr.Types)
                {
                    var cmdAttr = type.GetCustomAttribute<CommandAttribute>();
                    subcommands.Add(new SubcommandMetadata(type)
                    {
                        CommandName = cmdAttr?.Name,
                        MetadataProviderFactory = () => new ReflectionMetadataProvider(type)
                    });
                }
            }

            return subcommands;
        }

        private CommandMetadata? ExtractCommandInfo()
        {
            var attr = _modelType.GetCustomAttribute<CommandAttribute>();
            if (attr == null)
            {
                return null;
            }

            return new CommandMetadata
            {
                Name = attr.Name,
                AdditionalNames = attr.Names?.Skip(1).ToArray(),
                FullName = attr.FullName,
                Description = attr.Description,
                ExtendedHelpText = attr.ExtendedHelpText,
                ShowInHelpText = attr.ShowInHelpText,
                AllowArgumentSeparator = attr.AllowArgumentSeparator,
                // Only set ClusterOptions if explicitly specified in the attribute
                ClusterOptions = attr.ClusterOptionsWasSet ? attr.ClusterOptions : null,
                OptionsComparison = attr.OptionsComparison,
                ParseCulture = attr.ParseCulture,
                ResponseFileHandling = attr.ResponseFileHandling,
                // Only set UnrecognizedArgumentHandling if explicitly specified in the attribute
                UnrecognizedArgumentHandling = attr.UnrecognizedArgumentHandlingWasSet ? attr.UnrecognizedArgumentHandling : null,
                UsePagerForHelpText = attr.UsePagerForHelpText
            };
        }

        private IExecuteHandler? ExtractExecuteHandler()
        {
            MethodInfo? method;
            MethodInfo? asyncMethod;
            try
            {
                method = _modelType.GetMethod("OnExecute", MethodLookup);
                asyncMethod = _modelType.GetMethod("OnExecuteAsync", MethodLookup);
            }
            catch (AmbiguousMatchException)
            {
                // Return handler that throws when invoked
                return new ErrorExecuteHandler(Strings.AmbiguousOnExecuteMethod);
            }

            if (method != null && asyncMethod != null)
            {
                // Return handler that throws when invoked
                return new ErrorExecuteHandler(Strings.AmbiguousOnExecuteMethod);
            }

            method ??= asyncMethod;
            if (method == null)
            {
                return null; // No OnExecute method - not an error, just no handler
            }

            var isAsync = method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<int>);
            return new ReflectionExecuteHandler(method, isAsync);
        }

        private IValidateHandler? ExtractValidateHandler()
        {
            var method = _modelType.GetMethod("OnValidate", MethodLookup);
            if (method == null)
            {
                return null;
            }

            // Validate return type - must return ValidationResult
            if (method.ReturnType != typeof(ValidationResult))
            {
                throw new InvalidOperationException(Strings.InvalidOnValidateReturnType(_modelType));
            }

            return new ReflectionValidateHandler(method);
        }

        private IValidationErrorHandler? ExtractValidationErrorHandler()
        {
            var method = _modelType.GetMethod("OnValidationError", MethodLookup);
            if (method == null)
            {
                return null;
            }

            return new ReflectionValidationErrorHandler(method);
        }

        private SpecialPropertiesMetadata? ExtractSpecialProperties()
        {
            Action<object, object?>? parentSetter = null;
            Type? parentType = null;
            Action<object, object?>? subcommandSetter = null;
            Type? subcommandType = null;
            Action<object, object?>? remainingArgumentsSetter = null;
            Type? remainingArgumentsType = null;

            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            // Parent property - detected by name "Parent"
            var parentProp = _modelType.GetProperty("Parent", bindingFlags);
            if (parentProp != null)
            {
                var setter = ReflectionHelper.GetPropertySetter(parentProp);
                parentSetter = (obj, val) => setter(obj, val);
                parentType = parentProp.PropertyType;
            }

            // Subcommand property - detected by name "Subcommand"
            var subcommandProp = _modelType.GetProperty("Subcommand", bindingFlags);
            if (subcommandProp != null)
            {
                var setter = ReflectionHelper.GetPropertySetter(subcommandProp);
                subcommandSetter = (obj, val) => setter(obj, val);
                subcommandType = subcommandProp.PropertyType;
            }

            // RemainingArguments property - detected by name "RemainingArguments" or "RemainingArgs"
            var remainingProp = _modelType.GetProperty("RemainingArguments", bindingFlags);
            remainingProp ??= _modelType.GetProperty("RemainingArgs", bindingFlags);
            if (remainingProp != null)
            {
                var setter = ReflectionHelper.GetPropertySetter(remainingProp);
                remainingArgumentsSetter = (obj, val) => setter(obj, val);
                remainingArgumentsType = remainingProp.PropertyType;
            }

            if (parentSetter == null && subcommandSetter == null && remainingArgumentsSetter == null)
            {
                return null;
            }

            return new SpecialPropertiesMetadata
            {
                ParentSetter = parentSetter,
                ParentType = parentType,
                SubcommandSetter = subcommandSetter,
                SubcommandType = subcommandType,
                RemainingArgumentsSetter = remainingArgumentsSetter,
                RemainingArgumentsType = remainingArgumentsType
            };
        }

        private HelpOptionMetadata? ExtractHelpOption()
        {
            // Check type-level attribute
            var typeAttr = _modelType.GetCustomAttribute<HelpOptionAttribute>();
            if (typeAttr != null)
            {
                return new HelpOptionMetadata
                {
                    Template = typeAttr.Template,
                    Description = typeAttr.Description,
                    Inherited = typeAttr.Inherited
                };
            }

            // Check property-level attribute
            var props = ReflectionHelper.GetProperties(_modelType);
            foreach (var prop in props)
            {
                var propAttr = prop.GetCustomAttribute<HelpOptionAttribute>();
                if (propAttr != null)
                {
                    return new HelpOptionMetadata
                    {
                        Template = propAttr.Template,
                        Description = propAttr.Description,
                        Inherited = propAttr.Inherited
                    };
                }
            }

            return null;
        }

        private VersionOptionMetadata? ExtractVersionOption()
        {
            // Check type-level VersionOptionAttribute
            var typeAttr = _modelType.GetCustomAttribute<VersionOptionAttribute>();
            if (typeAttr != null)
            {
                return new VersionOptionMetadata
                {
                    Template = typeAttr.Template,
                    Description = typeAttr.Description,
                    Version = typeAttr.Version
                };
            }

            // Check type-level VersionOptionFromMemberAttribute
            var fromMemberAttr = _modelType.GetCustomAttribute<VersionOptionFromMemberAttribute>();
            if (fromMemberAttr != null)
            {
                Func<object, string?>? versionGetter = null;

                if (!string.IsNullOrEmpty(fromMemberAttr.MemberName))
                {
                    var members = ReflectionHelper.GetPropertyOrMethod(_modelType, fromMemberAttr.MemberName);
                    if (members.Length > 0)
                    {
                        var method = members[0];
                        versionGetter = obj => method.Invoke(obj, Array.Empty<object>()) as string;
                    }
                }

                return new VersionOptionMetadata
                {
                    Template = fromMemberAttr.Template,
                    Description = fromMemberAttr.Description,
                    VersionGetter = versionGetter
                };
            }

            // Check property-level VersionOptionAttribute
            var props = ReflectionHelper.GetProperties(_modelType);
            foreach (var prop in props)
            {
                var propAttr = prop.GetCustomAttribute<VersionOptionAttribute>();
                if (propAttr != null)
                {
                    var getter = ReflectionHelper.GetPropertyGetter(prop);
                    return new VersionOptionMetadata
                    {
                        Template = propAttr.Template,
                        Description = propAttr.Description,
                        Version = propAttr.Version,
                        VersionGetter = obj => getter(obj)?.ToString()
                    };
                }
            }

            return null;
        }
    }
}
