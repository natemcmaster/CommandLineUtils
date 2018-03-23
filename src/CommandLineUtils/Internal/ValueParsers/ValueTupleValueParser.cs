// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class ValueTupleValueParser<T> : IValueParser<(bool, T)>
    {
        private readonly IValueParser<T> _typeParser;

        public ValueTupleValueParser(IValueParser<T> typeParser)
        {
            _typeParser = typeParser;
        }

        public Type TargetType { get; } = typeof((bool, T));

        public (bool, T) Parse(string argName, string value, CultureInfo culture)
        {
            if (value == null)
            {
                return (true, default(T));
            }

            return (true, _typeParser.Parse(argName, value, culture));
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
