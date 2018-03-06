// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            foreach (var attr in prop.GetCustomAttributes().OfType<ValidationAttribute>())
            {
                option.Validators.Add(new AttributeValidator(attr));
            }

            if (option.OptionType == CommandOptionType.NoValue && prop.PropertyType != typeof(bool))
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

            var setter = ReflectionHelper.GetPropertySetter(prop);

            switch (option.OptionType)
            {
                case CommandOptionType.MultipleValue:
                    var collectionParser = CollectionParserProvider.Default.GetParser(prop.PropertyType);
                    if (collectionParser == null)
                    {
                        throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                    }
                    context.Application.OnParsingComplete(_ =>
                        setter.Invoke(context.ModelAccessor.GetModel(), collectionParser.Parse(option.LongName, option.Values)));
                    break;
                case CommandOptionType.SingleOrNoValue:
                    var valueTupleParser = ValueTupleParserProvider.Default.GetParser(prop.PropertyType);
                    if (valueTupleParser == null)
                    {
                        throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                    }
                    context.Application.OnParsingComplete(_ =>
                        setter.Invoke(context.ModelAccessor.GetModel(), valueTupleParser.Parse(option.HasValue(), option.LongName, option.Value())));
                    break;
                case CommandOptionType.SingleValue:
                    var parser = ValueParserProvider.Default.GetParser(prop.PropertyType);
                    if (parser == null)
                    {
                        throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                    }
                    context.Application.OnParsingComplete(_ =>
                    {
                        var value = option.Value();
                        if (value == null)
                        {
                            return;
                        }
                        setter.Invoke(context.ModelAccessor.GetModel(), parser.Parse(option.LongName, value));
                    });
                    break;
                case CommandOptionType.NoValue:
                    context.Application.OnParsingComplete(_ => setter.Invoke(context.ModelAccessor.GetModel(), option.HasValue()));
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
