// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;
    using System.Globalization;

    internal class StringValueParser : IValueParser
    {
        private StringValueParser()
        { }

        public static StringValueParser Singleton { get; } = new StringValueParser();

        public Type TargetType { get; } = typeof(string);

        public object Parse(string argName, string value, CultureInfo culture) => value;
    }
}
