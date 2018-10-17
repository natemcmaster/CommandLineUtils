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

        private delegate bool NumberParser<T>(string s, NumberStyles styles, IFormatProvider provider, out T result);

        private static IValueParser<T> Number<T>(NumberParser<T> parser, NumberStyles styles,
                                                 Func<string, string, FormatException> errorSelector) =>
            ValueParser.Create((value, culture) => parser(value, styles, culture.NumberFormat, out var result)
                                                 ? (true, result)
                                                 : default, errorSelector);

        const NumberStyles FloatingPointNumberStyles = NumberStyles.Float
                                                     | NumberStyles.AllowThousands;

        private static FormatException InvalidFloatingPointNumberException(string argName, string value) =>
            InvalidValueException(argName, $"'{value}' is not a valid floating-point number.");

        public static readonly IValueParser<double> Double = Number<double>(double.TryParse, FloatingPointNumberStyles, InvalidFloatingPointNumberException);
        public static readonly IValueParser<float>  Float  = Number<float> (float.TryParse , FloatingPointNumberStyles, InvalidFloatingPointNumberException);

        private static FormatException InvalidNumberException(string argName, string value) =>
            InvalidValueException(argName, $"'{value}' is not a valid number.");

        public static readonly IValueParser<short> Int16 = Number<short>(short.TryParse, NumberStyles.Integer, InvalidNumberException);
        public static readonly IValueParser<int>   Int32 = Number<int>  (int.TryParse  , NumberStyles.Integer, InvalidNumberException);
        public static readonly IValueParser<long>  Int64 = Number<long> (long.TryParse , NumberStyles.Integer, InvalidNumberException);

        const NumberStyles NonNegativeIntegerNumberStyles = NumberStyles.AllowLeadingWhite
                                                          | NumberStyles.AllowTrailingWhite;

        private static FormatException InvalidNonNegativeNumberException(string argName, string value) =>
            InvalidValueException(argName, $"'{value}' is not a valid, non-negative number.");

        public static readonly IValueParser<byte>   Byte   = Number<byte>  (byte.TryParse  , NonNegativeIntegerNumberStyles, InvalidNonNegativeNumberException);
        public static readonly IValueParser<ushort> UInt16 = Number<ushort>(ushort.TryParse, NonNegativeIntegerNumberStyles, InvalidNonNegativeNumberException);
        public static readonly IValueParser<uint>   UInt32 = Number<uint>  (uint.TryParse  , NonNegativeIntegerNumberStyles, InvalidNonNegativeNumberException);
        public static readonly IValueParser<ulong>  UInt64 = Number<ulong> (ulong.TryParse , NonNegativeIntegerNumberStyles, InvalidNonNegativeNumberException);

        public static readonly IValueParser<string> String = ValueParser.Create((_, value, __) => value);

        public static readonly IValueParser<Uri> Uri = ValueParser.Create(
            (_, value, culture) => new Uri(value, UriKind.RelativeOrAbsolute));
    }
}
