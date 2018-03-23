// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class FloatValueParser : IValueParser<float>
    {
        private FloatValueParser()
        { }

        public static FloatValueParser Singleton { get; } = new FloatValueParser();

        public Type TargetType { get; } = typeof(float);

        public float Parse(string argName, string value, CultureInfo culture)
        {
            if (value == null) return default;

            if (!float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid floating-point number.");
            }

            return result;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
