$content = Get-Content -Path CommandMetadataGenerator.cs -Raw

# Add validation extraction call to ExtractOptionData
$content = $content -replace '(// Infer OptionType from property type if not explicitly set\s+data\.InferredOptionType = InferOptionType\(property\.Type\);)\s+(return data;\s+\})\s+(/// <summary>\s+/// Infers CommandOptionType)', @'
$1

            // Extract validation attributes
            ExtractValidationAttributes(property, data.Validators);

            $2

        $3
'@

# Add validation extraction call to ExtractArgumentData (after ShowInHelpText)
$content = $content -replace '(case "ShowInHelpText":\s+data\.ShowInHelpText = \(bool\?\)arg\.Value\.Value;\s+break;\s+\}\s+\})\s+(return data;\s+\})\s+(private static HelpOptionData)', @'
$1

            // Extract validation attributes
            ExtractValidationAttributes(property, data.Validators);

            $2

        $3
'@

# Add ExtractValidationAttributes method after ExtractVersionOptionData (before ExtractSubcommandMetadata)
$content = $content -replace '(return data;\s+\})\s+(private static void ExtractSubcommandMetadata\(AttributeData attr, CommandInfo info\))', @'
$1

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

        $2
'@

Set-Content -Path CommandMetadataGenerator.cs -Value $content -NoNewline
