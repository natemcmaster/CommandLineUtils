// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.ValueParsers
{
    internal class NullableValueParser : IValueParser
    {
        private readonly IValueParser _wrapped;

        public NullableValueParser(IValueParser boxedParser)
        {
            _wrapped = boxedParser;
        }

        public object Parse(string argName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return _wrapped.Parse(argName, value);
        }
    }
}
