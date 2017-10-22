// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.ValueParsers
{
    internal class UInt16ValueParser : IValueParser
    {
        private UInt16ValueParser()
        { }

        public static UInt16ValueParser Singleton { get; } = new UInt16ValueParser();

        public object Parse(string argName, string value)
        {
            if (!ushort.TryParse(value, out var result))
            {
                throw new CommandParsingException(null, $"Invalid value specified for {argName}. '{value}' is not a valid, non-negative number.");
            }
            return result;
        }
    }
}
