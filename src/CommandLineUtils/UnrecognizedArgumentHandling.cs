// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Defines behaviors for for how unrecognized arguments should be handled.
    /// </summary>
    public enum UnrecognizedArgumentHandling
    {
        /// <summary>
        /// When an unrecognized argument is encountered, throw <see cref="CommandParsingException"/>.
        /// </summary>
        Throw = 0,

        /// <summary>
        /// When an unrecognized argument is encountered, stop parsing arguments and put all remaining arguments,
        /// including the first unrecognized argument, in <see cref="CommandLineApplication.RemainingArguments"/>.
        /// </summary>
        StopParsingAndCollect,
    }
}
