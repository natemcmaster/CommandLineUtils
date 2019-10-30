// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Configures the argument parser.
    /// </summary>
    public class ParserConfig
    {
        private char[] _optionNameValueSeparators = { ' ', ':', '=' };

        /// <summary>
        /// Characters used to separate the option name from the value.
        /// <para>
        /// By default, allowed separators are ' ' (space), :, and =
        /// </para>
        /// </summary>
        /// <remarks>
        /// Space actually implies multiple spaces due to the way most operating system shells parse command
        /// line arguments before starting a new process.
        /// </remarks>
        /// <example>
        /// Given --name=value, = is the separator.
        /// </example>
        public char[] OptionNameValueSeparators
        {
            get => _optionNameValueSeparators;
            set
            {
                _optionNameValueSeparators = value ?? throw new ArgumentNullException(nameof(value));
                if (value.Length == 0)
                {
                    throw new ArgumentException(Strings.IsNullOrEmpty, nameof(value));
                }
                OptionNameAndValueCanBeSpaceSeparated = Array.IndexOf(OptionNameValueSeparators, ' ') >= 0;
            }
        }

        internal bool OptionNameAndValueCanBeSpaceSeparated { get; private set; } = true;
    }
}
