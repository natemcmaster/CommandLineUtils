// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;

    internal class Int64ValueParser : IValueParser
    {
        private Int64ValueParser()
        { }

        public static Int64ValueParser Singleton { get; } = new Int64ValueParser();

        public Type TargetType { get; } = typeof(long);

        public object Parse(string argName, string value)
        {
            if (!long.TryParse(value, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid number.");
            }
            return result;
        }
    }
}
