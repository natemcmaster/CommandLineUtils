// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.ValueParsers
{
    internal class Int16ValueParser : IValueParser
    {
        private Int16ValueParser()
        { }

        public static Int16ValueParser Singleton { get; } = new Int16ValueParser();

        public object Parse(string argName, string value)
        {
            if (!short.TryParse(value, out var result))
            {
                throw new CommandParsingException(null, $"Invalid value specified for {argName}. '{value}' is not a valid number.");
            }
            return result;
        }
    }
}
