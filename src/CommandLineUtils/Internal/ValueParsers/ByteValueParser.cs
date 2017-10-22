// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.ValueParsers
{
    internal class ByteValueParser : IValueParser
    {
        private ByteValueParser()
        { }

        public static ByteValueParser Singleton { get; } = new ByteValueParser();

        public object Parse(string argName, string value)
        {
            if (!byte.TryParse(value, out var result))
            {
                throw new CommandParsingException(null, $"Invalid value specified for {argName}. '{value}' is not a valid number.");
            }
            return result;
        }
    }
}
