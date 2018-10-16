// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    partial class StockValueParsers
    {
        public static readonly IValueParser<ushort> UInt16 = ValueParser.Create(
            // TODO Fix NumberStyles to disallow leading/trailing sign
            (value, culture) => ushort.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            (argName, value) => new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid, non-negative number."));
    }

    internal static class UInt16ValueParser
    {
        public static IValueParser<ushort> Singleton { get; } = StockValueParsers.UInt16;
    }
}
