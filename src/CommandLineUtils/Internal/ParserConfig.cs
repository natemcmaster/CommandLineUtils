// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Configures the argument parser.
    /// </summary>
    internal class ParserConfig
    {
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
        public char[]? OptionNameValueSeparators { get; set; }

        /// <summary>
        /// Set the behavior for how to handle unrecognized arguments.
        /// </summary>
        public UnrecognizedArgumentHandling? UnrecognizedArgumentHandling { get; set; }
    }
}
