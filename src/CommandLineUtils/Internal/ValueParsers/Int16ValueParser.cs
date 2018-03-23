// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class Int16ValueParser : IValueParser<short>
    {
        private Int16ValueParser()
        { }

        public static Int16ValueParser Singleton { get; } = new Int16ValueParser();

        public Type TargetType { get; } = typeof(short);

        public short Parse(string argName, string value, CultureInfo culture)
        {
            if (value == null) return default;

            if (!short.TryParse(value, NumberStyles.Integer, culture, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid number.");
            }

            return result;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
