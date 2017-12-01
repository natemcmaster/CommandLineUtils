// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents one or many command line option that is identified by flag proceeded by '-' or '--'.
    /// Options are not positional. Compare to <see cref="ArgumentAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class OptionAttribute : OptionAttributeBase
    {
        /// <summary>
        /// Initializes a new <see cref="OptionAttribute"/>.
        /// </summary>
        public OptionAttribute()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="OptionAttribute"/>.
        /// </summary>
        /// <param name="template">The string template. <see cref="CommandOption.Template"/>.</param>
        public OptionAttribute(string template)
        {
            Template = template;
        }

        /// <summary>
        /// Initializes a new <see cref="OptionAttribute"/>.
        /// </summary>
        /// <param name="optionType">The optionType</param>
        public OptionAttribute(CommandOptionType optionType)
            : this(null, null, optionType)
        { }

        /// <summary>
        /// Initializes a new <see cref="OptionAttribute"/>.
        /// </summary>
        /// <param name="template">The template</param>
        /// <param name="optionType">The option type</param>
        public OptionAttribute(string template, CommandOptionType optionType)
            : this(template, null, optionType)
        { }

        /// <summary>
        /// Initializes a new <see cref="OptionAttribute"/>.
        /// </summary>
        /// <param name="template">The template</param>
        /// <param name="description">The option description</param>
        /// <param name="optionType">The option type</param>
        public OptionAttribute(string template, string description, CommandOptionType optionType)
        {
            Template = template;
            Description = description;
            OptionType = optionType;
        }

        /// <summary>
        /// Defines the type of the option. When not set, this will be inferred from the CLR type of the property. <seealso cref="CommandOption.OptionType"/>
        /// </summary>
        public CommandOptionType? OptionType { get; set; }

        internal CommandOption Configure(CommandLineApplication app, PropertyInfo prop)
        {
            var optionType = GetOptionType(prop);
            CommandOption option;
            if (Template != null)
            {
                option = new CommandOption(Template, optionType);
            }
            else
            {
                var longName = prop.Name.ToKebabCase();
                option = new CommandOption(optionType)
                {
                    LongName = longName,
                    ShortName = longName.Substring(0, 1),
                    ValueName = prop.Name.ToConstantCase(),
                };

                option.Template = $"-{option.ShortName}|--{option.LongName}";

                if (option.OptionType != CommandOptionType.NoValue)
                {
                    option.Template += $" <{option.ValueName.ToConstantCase()}>";
                }
            }

            Configure(option);

            if (option.Description == null)
            {
                option.Description = prop.Name;
            }

            app.Options.Add(option);
            return option;
        }

        private CommandOptionType GetOptionType(PropertyInfo prop)
        {
            CommandOptionType optionType;
            if (OptionType.HasValue)
            {
                optionType = OptionType.Value;
            }
            else if (!CommandOptionTypeMapper.Default.TryGetOptionType(prop.PropertyType, out optionType))
            {
                throw new InvalidOperationException(Strings.CannotDetermineOptionType(prop));
            }
            return optionType;
        }
    }
}
