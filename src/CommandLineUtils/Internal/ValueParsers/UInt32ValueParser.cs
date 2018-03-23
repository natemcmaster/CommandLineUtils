// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class UInt32ValueParser : IValueParser<uint>
    {
        private UInt32ValueParser()
        { }

        public static UInt32ValueParser Singleton { get; } = new UInt32ValueParser();

        public Type TargetType { get; } = typeof(uint);

        public uint Parse(string argName, string value, CultureInfo culture)
        {
            if (value == null) return default;

            if (!uint.TryParse(value, NumberStyles.Integer, culture, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid, non-negative number.");
            }
            return result;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
