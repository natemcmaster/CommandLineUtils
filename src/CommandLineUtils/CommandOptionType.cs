// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Defines the kinds of inputs <see cref="CommandOption"/> accepts.
    /// </summary>
    public enum CommandOptionType
    {
        /// <summary>
        /// The option can be specified multiple times.
        /// </summary>
        MultipleValue,

        /// <summary>
        /// The option can only be specified once.
        /// </summary>
        SingleValue,

        /// <summary>
        /// The option can only be specified once, and does not have a value.
        /// </summary>
        NoValue
    }
}
