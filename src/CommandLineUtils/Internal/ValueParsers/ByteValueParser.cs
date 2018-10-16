// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;
    using System.Globalization;

    partial class StockValueParsers
    {
        public static readonly IValueParser<byte> Byte = ValueParser.Create(
            (value, culture) => byte.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            (argName, value) => new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid number."));
    }

    internal static class ByteValueParser
    {
        public static IValueParser<byte> Singleton { get; } = StockValueParsers.Byte;
    }
}
