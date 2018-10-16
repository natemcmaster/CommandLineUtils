// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;
    using System.Globalization;

    internal static class StockValueParsers
    {
        public static readonly IValueParser<bool> Boolean = ValueParser.Create(
            (argName, value, culture) =>
            {
                if (value == null) return default;

                if (value == "T" || value == "t") return true;
                if (value == "F" || value == "f") return false;

                if (!bool.TryParse(value, out var result))
                {
                    if (short.TryParse(value, out var bit))
                    {
                        if (bit == 0) return false;
                        if (bit == 1) return true;
                    }

                    throw new FormatException($"Invalid value specified for {argName}. Cannot convert '{value}' to a boolean.");
                }

                return result;
            });

        private static FormatException InvalidValueException(string argName, string specifics) =>
            new FormatException($"Invalid value specified for {argName}. {specifics}");

        private static FormatException InvalidFloatingPointNumberException(string argName, string value) =>
            InvalidValueException(argName, $"'{value}' is not a valid floating-point number.");

        private static FormatException InvalidNumberException(string argName, string value) =>
            InvalidValueException(argName, $"'{value}' is not a valid number.");

        private static FormatException InvalidNonNegativeNumberException(string argName, string value) =>
            InvalidValueException(argName, $"'{value}' is not a valid, non-negative number.");

        public static readonly IValueParser<byte> Byte = ValueParser.Create(
            (value, culture) => byte.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            InvalidNumberException);

        public static readonly IValueParser<double> Double = ValueParser.Create(
            (value, culture) => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out var result) ? (true, result) : default,
            InvalidFloatingPointNumberException);

        public static readonly IValueParser<float> Float = ValueParser.Create(
            (value, culture) => float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out var result) ? (true, result) : default,
            InvalidFloatingPointNumberException);

        public static readonly IValueParser<short> Int16 = ValueParser.Create(
            (value, culture) => short.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            InvalidNumberException);

        public static readonly IValueParser<int> Int32 = ValueParser.Create(
            (value, culture) => int.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            InvalidNumberException);

        public static readonly IValueParser<long> Int64 = ValueParser.Create(
            (value, culture) => long.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            InvalidNumberException);

        public static readonly IValueParser<string> String = ValueParser.Create(
            (_, value, __) => value);

        public static readonly IValueParser<ushort> UInt16 = ValueParser.Create(
            // TODO Fix NumberStyles to disallow leading/trailing sign
            (value, culture) => ushort.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            InvalidNonNegativeNumberException);

        public static readonly IValueParser<uint> UInt32 = ValueParser.Create(
            // TODO Fix NumberStyles to disallow leading/trailing sign
            (value, culture) => uint.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            InvalidNonNegativeNumberException);

        public static readonly IValueParser<ulong> UInt64 = ValueParser.Create(
            // TODO Fix NumberStyles to disallow leading/trailing sign
            (value, culture) => ulong.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result) ? (true, result) : default,
            InvalidNonNegativeNumberException);

        public static readonly IValueParser<Uri> Uri = ValueParser.Create(
            (_, value, culture) => new Uri(value, UriKind.RelativeOrAbsolute));
    }
}
