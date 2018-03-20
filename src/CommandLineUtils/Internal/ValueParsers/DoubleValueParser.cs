// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;

    internal class DoubleValueParser : IValueParser
    {
        private DoubleValueParser()
        { }

        public static DoubleValueParser Singleton { get; } = new DoubleValueParser();

        public Type TargetType { get; } = typeof(double);

        public object Parse(string argName, string value)
        {
            if (!double.TryParse(value, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid floating-point number.");
            }
            return result;
        }
    }
}
