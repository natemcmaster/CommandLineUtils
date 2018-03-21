// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System.Globalization;

    internal class ValueTupleValueParser<T> : ITupleValueParser
    {
        private readonly IValueParser _typeParser;

        public ValueTupleValueParser(IValueParser typeParser)
        {
            _typeParser = typeParser;
        }

        public object Parse(bool hasValue, string argName, string value, CultureInfo culture)
        {
            if (!hasValue)
            {
                return (false, default(T));
            }

            if (value == null)
            {
                return (true, default(T));
            }

            return (true, (T)_typeParser.Parse(argName, value, culture));
        }
    }
}
