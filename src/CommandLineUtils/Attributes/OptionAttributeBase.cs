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
        /// The short command line flag used to identify this option. On command line, this is preceeded by a single '-{ShortName}'.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// The long command line flag used to identify this option. On command line, this is preceeded by a double dash: '--{LongName}'.
        /// </summary>
        public string LongName { get; set; }

        /// <summary>
        /// Can be used in addition to <see cref="ShortName"/> to add a single, non-English character.
        /// Example "-?".
        /// </summary>
        public string SymbolName { get; set; }

        /// <summary>
        /// The name of value(s) shown in help text when OptionType is not <see cref="CommandOptionType.NoValue"/>.
        /// </summary>
        public string ValueName { get; set; }

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

        internal void Configure(CommandOption option)
        {
            option.Description = Description ?? option.Description;
            option.Inherited = Inherited;
            option.ShowInHelpText = ShowInHelpText;
            option.ShortName = ShortName ?? option.ShortName;
            option.LongName = LongName ?? option.LongName;
            option.ValueName = ValueName ?? option.ValueName;
            option.SymbolName = SymbolName ?? option.SymbolName;
        }
    }
}
