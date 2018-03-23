// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class TupleValueParser<T> : IValueParser<Tuple<bool, T>>
    {
        private readonly IValueParser<T> _typeParser;

        public TupleValueParser(IValueParser<T> typeParser)
        {
            _typeParser = typeParser;
        }

        public Type TargetType { get; } = typeof(Tuple<bool, T>);

        public Tuple<bool, T> Parse(string argName, string value, CultureInfo culture)
        {
            if (value == null)
            {
                return Tuple.Create<bool, T>(false, default);
            }

            return Tuple.Create(true, _typeParser.Parse(argName, value, culture));
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
