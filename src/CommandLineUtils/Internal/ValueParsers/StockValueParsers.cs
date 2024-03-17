// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal static class StockValueParsers
    {
        public static readonly IValueParser<bool> Boolean = ValueParser.Create(
            (argName, value, culture) =>
            {
                switch (value)
                {
                    case null:
                        return true;
                    case "T":
                    case "t":
                        return true;
                    case "F":
                    case "f":
                        return false;
                }

                if (!bool.TryParse(value, out var result))
                {
                    if (short.TryParse(value, out var bit))
                    {
                        if (bit == 0) return false;
                        if (bit == 1) return true;
                    }

                    throw InvalidValueException(argName, $"Cannot convert '{value}' to a boolean.");
                }

                return result;
            });

        public static readonly IValueParser<string?> String = ValueParser.Create((_, value, __) => value);

        public static readonly IValueParser<Uri> Uri = ValueParser.Create(
#pragma warning disable CS8604 // Possible null reference argument.
            (_, value, culture) => new Uri(value, UriKind.RelativeOrAbsolute));
#pragma warning restore CS8604 // Possible null reference argument.

        private static FormatException InvalidValueException(string? argName, string specifics) =>
            new($"Invalid value specified for {argName}. {specifics}");

        private delegate bool NumberParser<T>(string s, NumberStyles styles, IFormatProvider provider, out T result);

        private static IValueParser<T> Create<T>(NumberParser<T> parser, NumberStyles styles,
                                                 Func<string?, string?, FormatException> errorSelector)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (errorSelector == null) throw new ArgumentNullException(nameof(errorSelector));

            return ValueParser.Create((value, culture) => parser(value, styles, culture.NumberFormat, out var result)
                                                        ? (true, result)
                                                        : default,
                                      errorSelector);
        }

        private static IValueParser<T> FloatingPointParser<T>(NumberParser<T> parser) =>
            Create(parser, NumberStyles.Float
                         | NumberStyles.AllowThousands,
                   (argName, value) => InvalidValueException(argName, $"'{value}' is not a valid floating-point number."));

        public static readonly IValueParser<double> Double = FloatingPointParser<double>(double.TryParse);
        public static readonly IValueParser<float> Float = FloatingPointParser<float>(float.TryParse);

        private static IValueParser<T> IntegerParser<T>(NumberParser<T> parser) =>
            Create(parser, NumberStyles.Integer, (argName, value) => InvalidValueException(argName, $"'{value}' is not a valid number."));

        public static readonly IValueParser<short> Int16 = IntegerParser<short>(short.TryParse);
        public static readonly IValueParser<int> Int32 = IntegerParser<int>(int.TryParse);
        public static readonly IValueParser<long> Int64 = IntegerParser<long>(long.TryParse);

        private static IValueParser<T> NonNegativeIntegerParser<T>(NumberParser<T> parser) =>
            Create(parser, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                   (argName, value) => InvalidValueException(argName, $"'{value}' is not a valid, non-negative number."));

        public static readonly IValueParser<byte> Byte = NonNegativeIntegerParser<byte>(byte.TryParse);
        public static readonly IValueParser<ushort> UInt16 = NonNegativeIntegerParser<ushort>(ushort.TryParse);
        public static readonly IValueParser<uint> UInt32 = NonNegativeIntegerParser<uint>(uint.TryParse);
        public static readonly IValueParser<ulong> UInt64 = NonNegativeIntegerParser<ulong>(ulong.TryParse);

        private delegate bool DateTimeParser<T>(string s, IFormatProvider provider, DateTimeStyles styles, out T result);

        private static IValueParser<T> Create<T>(DateTimeParser<T> parser, DateTimeStyles styles, Func<string?, string?, FormatException> errorSelector)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (errorSelector == null) throw new ArgumentNullException(nameof(errorSelector));

            return ValueParser.Create((value, culture) => parser(value, culture.DateTimeFormat, styles, out var result)
                                                        ? (true, result)
                                                        : default,
                                      errorSelector);
        }

        public static readonly IValueParser<DateTime> DateTime = Create<DateTime>(System.DateTime.TryParse, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces,
            (argName, value) => InvalidValueException(argName, $"'{value}' is not a valid date-time."));

        public static readonly IValueParser<DateTimeOffset> DateTimeOffset = Create<DateTimeOffset>(System.DateTimeOffset.TryParse, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces,
            (argName, value) => InvalidValueException(argName, $"'{value}' is not a valid date-time (with offset)."));

        public static readonly IValueParser<TimeSpan> TimeSpan = ValueParser.Create(
            (value, culture) => System.TimeSpan.TryParse(value, culture, out var result) ? (true, result) : default,
            (argName, value) => InvalidValueException(argName, $"'{value}' is not a valid time-span."));
    }
}
