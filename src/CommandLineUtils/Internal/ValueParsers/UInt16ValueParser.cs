// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class UInt16ValueParser : IValueParser<ushort>
    {
        private UInt16ValueParser()
        { }

        public static UInt16ValueParser Singleton { get; } = new UInt16ValueParser();

        public Type TargetType { get; } = typeof(ushort);

        public ushort Parse(string argName, string value, CultureInfo culture)
        {
            if (value == null) return default;

            if (!ushort.TryParse(value, NumberStyles.Integer, culture, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid, non-negative number.");
            }
            return result;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
