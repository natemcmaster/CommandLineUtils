// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// A command-option that can be directly parsed by <see cref="CommandLineApplication.Parse"/>
    /// </summary>
    public interface IParseableOption : IOption
    {
        /// <summary>
        /// The short command line flag used to identify this option. On command line, this is preceded by a single '-{ShortName}'.
        /// </summary>
        string? ShortName { get; set; }

        /// <summary>
        /// The long command line flag used to identify this option. On command line, this is preceded by a double dash: '--{LongName}'.
        /// </summary>
        string? LongName { get; set; }

        /// <summary>
        /// Can be used in addition to <see cref="ShortName"/> to add a single, non-English character.
        /// Example "-?".
        /// </summary>
        string? SymbolName { get; set; }

        /// <summary>
        /// The name of value(s) shown in help text when <see cref="IOption.OptionType"/> is not <see cref="CommandOptionType.NoValue"/>.
        /// </summary>
        string? ValueName { get; set; }

        /// <summary>
        /// Determines if this option should be shown in generated help text.
        /// </summary>
        bool ShowInHelpText { get; set; }
    }
}
