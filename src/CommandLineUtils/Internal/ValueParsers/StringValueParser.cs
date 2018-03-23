// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class StringValueParser : IValueParser<string>
    {
        private StringValueParser()
        { }

        public static StringValueParser Singleton { get; } = new StringValueParser();

        public Type TargetType { get; } = typeof(string);

        public string Parse(string argName, string value, CultureInfo culture) => value;

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
