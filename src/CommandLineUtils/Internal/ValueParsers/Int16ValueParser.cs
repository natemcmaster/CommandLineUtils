// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;
    using System.Globalization;

    internal class Int16ValueParser : IValueParser
    {
        private Int16ValueParser()
        { }

        public static Int16ValueParser Singleton { get; } = new Int16ValueParser();

        public Type TargetType { get; } = typeof(short);

        public object Parse(string argName, string value, CultureInfo culture)
        {
            if (!short.TryParse(value, NumberStyles.Integer, culture, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid number.");
            }

            return result;
        }
    }
}
