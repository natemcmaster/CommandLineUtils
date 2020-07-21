// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// A parser that can convert string into an object.
    /// </summary>
    public interface IValueParser
    {
        /// <summary>
        /// Gets the Type that this value parser is defined for.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Parses the raw string value.
        /// </summary>
        /// <param name="argName">The name of the argument this value will be bound to.</param>
        /// <param name="value">The raw string value to parse.</param>
        /// <param name="culture">The culture that should be used to parse values.</param>
        /// <returns>The parsed value object.</returns>
        /// <throws name="System.FormatException">When the value cannot be parsed.</throws>
        object? Parse(string? argName, string? value, CultureInfo culture);
    }
}
