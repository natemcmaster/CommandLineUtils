// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Shared implementation for adding conventions based on <see cref="OptionAttributeBase"/>.
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class OptionAttributeConventionBase<TAttribute>
        where TAttribute : OptionAttributeBase
    {
        private protected void AddOption(ConventionContext context, CommandOption option, PropertyInfo prop)
        {
            var modelAccessor = context.ModelAccessor;
            if (modelAccessor == null)
            {
                throw new InvalidOperationException(Strings.ConventionRequiresModel);
            }

            foreach (var attr in prop.GetCustomAttributes().OfType<ValidationAttribute>())
            {
                option.Validators.Add(new AttributeValidator(attr));
            }

            if (option.OptionType == CommandOptionType.NoValue
                && prop.PropertyType != typeof(bool)
                && prop.PropertyType != typeof(bool?)
                && prop.PropertyType != typeof(bool[]))
            {
                throw new InvalidOperationException(Strings.NoValueTypesMustBeBoolean);
            }

            if (!string.IsNullOrEmpty(option.ShortName))
            {
                if (context.Application._shortOptions.TryGetValue(option.ShortName, out var otherProp))
                {
                    throw new InvalidOperationException(
                        Strings.OptionNameIsAmbiguous(option.ShortName, prop, otherProp));
                }
                context.Application._shortOptions.Add(option.ShortName, prop);
            }

            if (!string.IsNullOrEmpty(option.LongName))
            {
                if (context.Application._longOptions.TryGetValue(option.LongName, out var otherProp))
                {
                    throw new InvalidOperationException(
                        Strings.OptionNameIsAmbiguous(option.LongName, prop, otherProp));
                }
                context.Application._longOptions.Add(option.LongName, prop);
            }

            var getter = ReflectionHelper.GetPropertyGetter(prop);
            var setter = ReflectionHelper.GetPropertySetter(prop);

            switch (option.OptionType)
            {
                case CommandOptionType.MultipleValue:
                    context.Application.OnParsingComplete(_ =>
                    {
                        var collectionParser =
                            CollectionParserProvider.Default.GetParser(prop.PropertyType,
                                context.Application.ValueParsers);

                        if (collectionParser == null)
                        {
                            throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                        }

                        if (!option.HasValue())
                        {
                            if (!ReflectionHelper.IsSpecialValueTupleType(prop.PropertyType, out var type))
                            {
                                if (getter.Invoke(modelAccessor.GetModel()) is IEnumerable<object> values)
                                {
                                    foreach (var value in values)
                                    {
                                        option.TryParse(value?.ToString());
                                    }
                                    option.DefaultValue = string.Join(", ", values.Select(x => x?.ToString()));
                                }
                            }
                        }
                        else
                        {
                            setter.Invoke(modelAccessor.GetModel(), collectionParser.Parse(option.LongName, option.Values));
                        }
                    });
                    break;
                case CommandOptionType.SingleOrNoValue:
                case CommandOptionType.SingleValue:
                    context.Application.OnParsingComplete(_ =>
                    {
                        var parser = context.Application.ValueParsers.GetParser(prop.PropertyType);
                        if (parser == null)
                        {
                            throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                        }

                        if (!option.HasValue())
                        {
                            if (!ReflectionHelper.IsSpecialValueTupleType(prop.PropertyType, out var type))
                            {
                                var value = getter.Invoke(modelAccessor.GetModel());

                                if (value != null)
                                {
                                    option.TryParse(value.ToString());
                                    option.DefaultValue = value.ToString();
                                }
                            }
                        }
                        else
                        {
                            setter.Invoke(modelAccessor.GetModel(), parser.Parse(option.LongName, option.Value(), context.Application.ValueParsers.ParseCulture));
                        }
                    });
                    break;
                case CommandOptionType.NoValue:
                    context.Application.OnParsingComplete(_ =>
                    {
                        if (prop.PropertyType == typeof(bool[]))
                        {
                            if (!option.HasValue())
                            {
                                setter.Invoke(modelAccessor.GetModel(), Array.Empty<bool>());
                            }

                            var count = new bool[option.Values.Count];
                            for (var i = 0; i < count.Length; i++)
                            {
                                count[i] = true;
                            }

                            setter.Invoke(modelAccessor.GetModel(), count);
                        }
                        else
                        {
                            if (!option.HasValue())
                            {
                                return;
                            }

                            setter.Invoke(modelAccessor.GetModel(), option.HasValue());
                        }
                    });
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private protected static void EnsureDoesNotHaveArgumentAttribute(PropertyInfo prop)
        {
            var argumentAttr = prop.GetCustomAttribute<ArgumentAttribute>();
            if (argumentAttr != null)
            {
                throw new InvalidOperationException(
                    Strings.BothOptionAndArgumentAttributesCannotBeSpecified(prop));
            }
        }
    }
}
