// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    partial class StockValueParsers
    {
        public static readonly IValueParser<long> Int64 = ValueParser.Create(
            (value, culture) => long.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            (argName, value) => new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid number."));
    }

    internal static class Int64ValueParser
    {
        public static IValueParser<long> Singleton { get; } = StockValueParsers.Int64;
    }
}
