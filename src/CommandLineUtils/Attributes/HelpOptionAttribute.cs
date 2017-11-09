// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// The option used to determine if help text should be displayed. This should only be used once per command line app.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class HelpOptionAttribute : OptionAttributeBase
    {
        /// <summary>
        /// Initializes a new <see cref="HelpOptionAttribute"/> with the template <c>-?|-h|--help</c>.
        /// </summary>
        public HelpOptionAttribute()
            : this(Strings.DefaultHelpTemplate)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="HelpOptionAttribute"/>.
        /// </summary>
        /// <param name="template">The string template. <see cref="CommandOption.Template"/>.</param>
        public HelpOptionAttribute(string template)
        {
            Template = template;
            Description = Strings.DefaultHelpOptionDescription;
        }

        internal CommandOption Configure(CommandLineApplication app)
        {
            var opt = app.HelpOption(Template);
            Configure(opt);
            return opt;
        }
    }
}
