// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace McMaster.Extensions.CommandLineUtils.ValueParsers
{
    internal class StringValueParser : IValueParser
    {
        private StringValueParser()
        { }

        public static StringValueParser Singleton { get; } = new StringValueParser();

        public object Parse(string argName, string value) => value;
    }
}
