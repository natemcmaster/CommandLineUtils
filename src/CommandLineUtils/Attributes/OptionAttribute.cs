// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents one or many command line option that is identified by flag proceeded by '-' or '--'.
    /// Options are not positional. Compare to <see cref="ArgumentAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
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
        {
            OptionType = optionType;
        }

        /// <summary>
        /// Initializes a new <see cref="OptionAttribute"/>.
        /// </summary>
        /// <param name="template">The template</param>
        /// <param name="optionType">The option type</param>
        public OptionAttribute(string template, CommandOptionType optionType)
        {
            Template = template;
            OptionType = optionType;
        }

        /// <summary>
        /// The option template. This is parsed into the short and long name.
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// A description of this option to show in generated help text. <seealso cref="CommandOption.Description"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Defines the type of the option. When not set, this will be inferred from the CLR type of the property. <seealso cref="CommandOption.OptionType"/>
        /// </summary>
        public CommandOptionType? OptionType { get; set; }

        /// <summary>
        /// Determines if this option should be shown in generated help text. <seealso cref="CommandOption.ShowInHelpText"/>.
        /// </summary>
        public bool ShowInHelpText { get; set; } = true;

        /// <summary>
        /// Determines if subcommands added to <see cref="CommandLineApplication.Commands"/>
        /// should also have access to this option. <seealso cref="CommandOption.Inherited"/>.
        /// </summary>
        public bool Inherited { get; set; }
    }
}
