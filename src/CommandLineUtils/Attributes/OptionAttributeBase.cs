// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Common option properties.
    /// </summary>
    public abstract class OptionAttributeBase : Attribute
    {
        /// <summary>
        /// The option template. This is parsed into the short and long name.
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// A description of this option to show in generated help text. <seealso cref="CommandOption.Description"/>.
        /// </summary>
        public string Description { get; set; }

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
