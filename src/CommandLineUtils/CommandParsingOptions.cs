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
        /// Throw when unexpected arguments are encountered
        /// </summary>
        ThrowOnUnexpectedArgs = 1 << 0,

        /// <summary>
        /// Don't set any options
        /// </summary>
        None = 0,
    }
}
