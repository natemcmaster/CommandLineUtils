// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Adds an <see cref="CommandOption"/> to match each usage of <see cref="OptionAttribute"/>
    /// on the model type of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class OptionAttributeConvention : OptionAttributeConventionBase<OptionAttribute>, IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            // MetadataProvider is always available (generated or reflection-based via DefaultMetadataResolver)
            var provider = context.MetadataProvider;
            if (provider == null)
            {
                return;
            }

            // Track options added by this provider to detect same-class conflicts
            var addedShortOptions = new System.Collections.Generic.Dictionary<string, OptionMetadata>(StringComparer.OrdinalIgnoreCase);
            var addedLongOptions = new System.Collections.Generic.Dictionary<string, OptionMetadata>(StringComparer.OrdinalIgnoreCase);

            foreach (var optMeta in provider.Options)
            {
                // Calculate the option names first to check for duplicates BEFORE adding
                var (template, shortName, longName) = GetOptionNames(optMeta);

                // Check for same-class conflicts (options in the same provider with conflicting names)
                if (!string.IsNullOrEmpty(shortName) && addedShortOptions.TryGetValue(shortName, out var existingShort))
                {
                    throw new InvalidOperationException(
                        Strings.OptionNameIsAmbiguous(shortName, optMeta.PropertyName, optMeta.DeclaringType, existingShort.PropertyName, existingShort.DeclaringType));
                }
                if (!string.IsNullOrEmpty(longName) && addedLongOptions.TryGetValue(longName, out var existingLong))
                {
                    throw new InvalidOperationException(
                        Strings.OptionNameIsAmbiguous(longName, optMeta.PropertyName, optMeta.DeclaringType, existingLong.PropertyName, existingLong.DeclaringType));
                }

                // Check if option already exists from parent command (inherited options)
                if (!string.IsNullOrEmpty(shortName) && context.Application._shortOptions.ContainsKey(shortName))
                {
                    continue; // Skip - option already registered by parent
                }
                if (!string.IsNullOrEmpty(longName) && context.Application._longOptions.ContainsKey(longName))
                {
                    continue; // Skip - option already registered by parent
                }

                // Track this option
                if (!string.IsNullOrEmpty(shortName))
                {
                    addedShortOptions[shortName] = optMeta;
                }
                if (!string.IsNullOrEmpty(longName))
                {
                    addedLongOptions[longName] = optMeta;
                }

                var option = CreateOptionFromMetadata(context.Application, optMeta, template);
                AddOptionFromMetadata(context, option, optMeta);
            }
        }

        private static (string template, string? shortName, string? longName) GetOptionNames(OptionMetadata meta)
        {
            var template = meta.Template;
            string? shortName = meta.ShortName;
            string? longName = meta.LongName;

            if (string.IsNullOrEmpty(template))
            {
                // Build template from ShortName/LongName
                if (!string.IsNullOrEmpty(shortName) && !string.IsNullOrEmpty(longName))
                {
                    template = $"-{shortName}|--{longName}";
                }
                else if (!string.IsNullOrEmpty(longName))
                {
                    template = $"--{longName}";
                }
                else if (!string.IsNullOrEmpty(shortName))
                {
                    template = $"-{shortName}";
                }
                else
                {
                    // Use property name as default
                    longName = meta.PropertyName.ToLowerInvariant();
                    template = $"--{longName}";
                }
            }
            else
            {
                // Parse short/long names from template if not already set
                if (string.IsNullOrEmpty(shortName) || string.IsNullOrEmpty(longName))
                {
                    var parts = template.Split('|');
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (trimmed.StartsWith("--") && string.IsNullOrEmpty(longName))
                        {
                            longName = trimmed.Substring(2).Split(' ', '<', ':', '=')[0];
                        }
                        else if (trimmed.StartsWith("-") && string.IsNullOrEmpty(shortName))
                        {
                            shortName = trimmed.Substring(1).Split(' ', '<', ':', '=')[0];
                        }
                    }
                }
            }

            return (template, shortName, longName);
        }

        private static CommandOption CreateOptionFromMetadata(CommandLineApplication app, OptionMetadata meta, string template)
        {
            // Validate that the property type can be parsed, but only if OptionType was NOT explicitly set.
            // When OptionType is explicitly set, the user knows what they're doing and may add a custom parser later.
            if (!meta.OptionTypeExplicitlySet && meta.OptionType != CommandOptionType.NoValue)
            {
                if (!CommandOptionTypeMapper.Default.TryGetOptionType(meta.PropertyType, app.ValueParsers, out _)
                    && app.ValueParsers.GetParser(meta.PropertyType) == null)
                {
                    throw new InvalidOperationException(Strings.CannotDetermineOptionType(meta.PropertyName, meta.PropertyType, meta.DeclaringType));
                }
            }

            // Use the option type from metadata (already correctly set from attribute or inferred)
            var option = app.Option(template, meta.Description ?? string.Empty, meta.OptionType);

            // Apply explicit ShortName/LongName from metadata (may be empty string to explicitly unset)
            // null means use what was parsed from template, non-null overrides it
            if (meta.ShortName != null)
            {
                option.ShortName = meta.ShortName;
            }
            if (meta.LongName != null)
            {
                option.LongName = meta.LongName;
            }
            if (!string.IsNullOrEmpty(meta.ValueName))
            {
                option.ValueName = meta.ValueName;
            }

            option.ShowInHelpText = meta.ShowInHelpText;
            option.Inherited = meta.Inherited;

            // Set underlying type for help text generator (enum allowed values display)
            option.UnderlyingType = meta.PropertyType;

            return option;
        }

        private void AddOptionFromMetadata(ConventionContext context, CommandOption option, OptionMetadata meta)
        {
            var modelAccessor = context.ModelAccessor;
            if (modelAccessor == null)
            {
                throw new InvalidOperationException(Strings.ConventionRequiresModel);
            }

            // Apply validation attributes from metadata
            foreach (var validator in meta.Validators)
            {
                option.Validators.Add(new Validation.AttributeValidator(validator));
            }

            // Register names for duplicate checking
            if (!string.IsNullOrEmpty(option.ShortName))
            {
                context.Application._shortOptions.TryAdd(option.ShortName, null!);
            }

            if (!string.IsNullOrEmpty(option.LongName))
            {
                context.Application._longOptions.TryAdd(option.LongName, null!);
            }

            // Use the generated getter/setter delegates
            var getter = meta.Getter;
            var setter = meta.Setter;

            switch (option.OptionType)
            {
                case CommandOptionType.MultipleValue:
                    context.Application.OnParsingComplete(r =>
                    {
                        var collectionParser = CollectionParserProvider.Default.GetParser(
                            meta.PropertyType, context.Application.ValueParsers);

                        if (collectionParser == null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot determine parser type for property '{meta.PropertyName}'");
                        }

                        if (!option.HasValue())
                        {
                            // Read the initial property value and use as default
                            if (!ReflectionHelper.IsSpecialValueTupleType(meta.PropertyType, out _))
                            {
                                if (getter(modelAccessor.GetModel()) is System.Collections.IEnumerable values
                                    && meta.PropertyType != typeof(string))
                                {
                                    var valueList = new System.Collections.Generic.List<string>();
                                    foreach (var value in values)
                                    {
                                        if (value != null)
                                        {
                                            valueList.Add(value.ToString() ?? string.Empty);
                                            option.TryParse(value.ToString());
                                        }
                                    }
                                    if (valueList.Count > 0)
                                    {
                                        option.DefaultValue = string.Join(", ", valueList);
                                    }
                                }
                            }
                        }
                        else
                        {
                            setter(modelAccessor.GetModel(), collectionParser.Parse(option.LongName, option.Values));
                        }
                    });
                    break;

                case CommandOptionType.SingleOrNoValue:
                case CommandOptionType.SingleValue:
                    context.Application.OnParsingComplete(r =>
                    {
                        var parser = context.Application.ValueParsers.GetParser(meta.PropertyType);
                        if (parser == null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot determine parser type for property '{meta.PropertyName}'");
                        }

                        if (!option.HasValue())
                        {
                            // Read the initial property value and use as default
                            if (!ReflectionHelper.IsSpecialValueTupleType(meta.PropertyType, out _))
                            {
                                var value = getter(modelAccessor.GetModel());
                                if (value != null)
                                {
                                    option.TryParse(value.ToString());
                                    option.DefaultValue = value.ToString();
                                }
                            }
                        }
                        else
                        {
                            setter(modelAccessor.GetModel(),
                                parser.Parse(option.LongName, option.Value(), context.Application.ValueParsers.ParseCulture));
                        }
                    });
                    break;

                case CommandOptionType.NoValue:
                    context.Application.OnParsingComplete(r =>
                    {
                        if (meta.PropertyType == typeof(bool[]))
                        {
                            if (!option.HasValue())
                            {
                                setter(modelAccessor.GetModel(), Array.Empty<bool>());
                                return;
                            }

                            var count = new bool[option.Values.Count];
                            for (var i = 0; i < count.Length; i++)
                            {
                                count[i] = true;
                            }
                            setter(modelAccessor.GetModel(), count);
                        }
                        else
                        {
                            if (option.HasValue())
                            {
                                setter(modelAccessor.GetModel(), option.HasValue());
                            }
                        }
                    });
                    break;
            }
        }

    }
}
