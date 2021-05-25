// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Option of the application
    /// </summary>
    /// <typeparam name="T">type of the <see cref="ParsedValues"/></typeparam>
    public interface IOption<T> : IOption
    {
        /// <summary>
        /// The parsed value.
        /// </summary>
        T ParsedValue { get; }

        /// <summary>
        /// All parsed values;
        /// </summary>
        IReadOnlyList<T> ParsedValues { get; }
    }
}
