// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;
    using System.Globalization;

    internal class ByteValueParser : IValueParser<byte>
    {
        private ByteValueParser()
        { }

        public static ByteValueParser Singleton { get; } = new ByteValueParser();

        public Type TargetType { get; } = typeof(byte);

        public byte Parse(string argName, string value, CultureInfo culture)
        {
            if (value == null) return default;

            if (!byte.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid number.");
            }

            return result;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
