// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class Int64ValueParser : IValueParser<long>
    {
        private Int64ValueParser()
        { }

        public static Int64ValueParser Singleton { get; } = new Int64ValueParser();

        public Type TargetType { get; } = typeof(long);

        public long Parse(string argName, string value, CultureInfo culture)
        {
            if (value == null) return default;

            if (!long.TryParse(value, NumberStyles.Integer, culture.NumberFormat, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid number.");
            }

            return result;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
