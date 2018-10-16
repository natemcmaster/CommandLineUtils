// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    partial class StockValueParsers
    {
        public static readonly IValueParser<ulong> UInt64 = ValueParser.Create(
            // TODO Fix NumberStyles to disallow leading/trailing sign
            (value, culture) => ulong.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            (argName, value) => new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid number."));
    }

    internal static class UInt64ValueParser
    {
        public static IValueParser<ulong> Singleton { get; } = StockValueParsers.UInt64;
    }
}
