// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents options for how the command line arguments should be parsed.
    /// </summary>
    [Flags]
    public enum CommandParsingOptions
    {
        /// <summary>
        /// Throw when unexpected arguments are encountered. <seealso cref="CommandLineApplication.ThrowOnUnexpectedArgument"/>
        /// </summary>
        ThrowOnUnexpectedArgument = 1 << 0,

        /// <summary>
        /// Allow '--' to be used to stop parsing arguments. <seealso cref="CommandLineApplication.AllowArgumentSeparator"/>
        /// </summary>
        AllowArgumentSeparator = 1 << 1,

        /// <summary>
        /// Treat arguments beginning as '@' as a response file. <seealso cref="CommandLineApplication.HandleResponseFiles"/>
        /// </summary>
        HandleResponseFiles = 1 << 2,

        /// <summary>
        /// Don't set any options
        /// </summary>
        None = 0,
    }
}
