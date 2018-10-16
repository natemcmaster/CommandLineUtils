// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    partial class StockValueParsers
    {
        public static readonly IValueParser<double> Double = ValueParser.Create(
            (value, culture) => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out var result) ? (true, result) : default,
            (argName, value) => new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid floating-point number."));
    }

    internal static class DoubleValueParser
    {
        public static IValueParser<double> Singleton { get; } = StockValueParsers.Double;
    }
}
