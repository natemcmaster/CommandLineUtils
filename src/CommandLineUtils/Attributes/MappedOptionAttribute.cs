// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Attributes
{
    /// <summary>
    /// Represents one or many command line option that is identified by flag proceeded by '-' or '--'.
    /// Options are not positional. Compare to <see cref="ArgumentAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MappedOptionAttribute : OptionAttributeBase
    {
        /// <summary>
        /// Initializes a new <see cref="MappedOptionAttribute"/>.
        /// </summary>
        public MappedOptionAttribute(object constantValue)
        {
            ConstantValue = constantValue;
        }

        /// <summary>
        /// Initializes a new <see cref="MappedOptionAttribute"/>.
        /// </summary>
        /// <param name="template">The string template. This is parsed into <see cref="CommandOption.ShortName"/> and <see cref="CommandOption.LongName"/>.</param>
        /// <param name="constantValue">The value to assign the the option is given.</param>
        public MappedOptionAttribute(string template, object constantValue)
        {
            Template = template;
            ConstantValue = constantValue;
        }

        /// <summary>
        /// Initializes a new <see cref="MappedOptionAttribute"/>.
        /// </summary>
        /// <param name="template">The template</param>
        /// <param name="description">The option description</param>
        /// <param name="constantValue">The value to assign the the option is given.</param>
        public MappedOptionAttribute(string template, string? description, object constantValue)
        {
            Template = template;
            Description = description;
            ConstantValue = constantValue;
        }

        /// <summary>
        /// Defines the type of the option. When not set, this will be inferred from the CLR type of the property.
        /// </summary>
        /// <seealso cref="CommandOption.OptionType"/>
        public CommandOptionType? OptionType { get; set; }

        /// <summary>
        /// Defines the value assigned to the property when the option is given.
        /// </summary>
        /// <seealso cref="ConstantValueOption{T}.ConstantValue"/>
        public object? ConstantValue { get; set; }

        internal CommandOption Configure<T>(MappedOption<T> mappedOption, PropertyInfo prop)
        {
            T constantValue;
            try
            {
                constantValue = (T)ConstantValue!;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(Strings.CannotDetermineOptionType(prop), e);
            }
            ConstantValueOption<T> option;
            if (Template != null)
            {
                option = mappedOption.Add(Template, constantValue);
            }
            else
            {
                option = mappedOption.Add(constantValue);
                var stringValue = constantValue?.ToString().ToKebabCase();
                if (stringValue != null)
                {
                    option.LongName = stringValue;
                    option.ShortName = stringValue.Substring(0, 1);
                }
            }

            Configure(option);
            return option;
        }
    }
}
