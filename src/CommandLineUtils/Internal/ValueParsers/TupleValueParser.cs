// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System.Globalization;

    internal class TupleValueParser<T> : ITupleValueParser
    {
        private readonly IValueParser _typeParser;

        public TupleValueParser(IValueParser typeParser)
        {
            _typeParser = typeParser;
        }

        public object Parse(bool hasValue, string argName, string value, CultureInfo culture)
        {
            if (!hasValue)
            {
                return Tuple.Create<bool, T>(false, default);
            }

            if (value == null)
            {
                return Tuple.Create<bool, T>(true, default);
            }

            return Tuple.Create(true, (T)_typeParser.Parse(argName, value, culture));
        }
    }
}
