// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;
    using System.Globalization;

    internal class UInt32ValueParser : IValueParser
    {
        private UInt32ValueParser()
        { }

        public static UInt32ValueParser Singleton { get; } = new UInt32ValueParser();

        public Type TargetType { get; } = typeof(uint);

        public object Parse(string argName, string value, CultureInfo culture)
        {
            if (!uint.TryParse(value, NumberStyles.Integer, culture, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid, non-negative number.");
            }
            return result;
        }
    }
}
