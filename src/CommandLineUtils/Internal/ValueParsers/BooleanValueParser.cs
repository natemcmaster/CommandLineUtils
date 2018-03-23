// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;
    using System.Globalization;

    internal class BooleanValueParser : IValueParser<bool>
    {
        private BooleanValueParser()
        { }

        public static BooleanValueParser Singleton { get; } = new BooleanValueParser();

        public Type TargetType { get; } = typeof(bool);

        public bool Parse(string argName, string value, CultureInfo culture)
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
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
