// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System.Globalization;

    internal class ArrayParser : ICollectionParser
    {
        private readonly Type _elementType;
        private readonly IValueParser _elementParser;
        private readonly CultureInfo _parserCulture;

        public ArrayParser(Type elementType, IValueParser elementParser, CultureInfo parserCulture)
        {
            _elementType = elementType;
            _elementParser = elementParser;
            _parserCulture = parserCulture;
        }

        public object Parse(string? argName, IReadOnlyList<string?> values)
        {
            var array = Array.CreateInstance(_elementType, values.Count);
            for (var i = 0; i < values.Count; i++)
            {
                array.SetValue(_elementParser.Parse(argName, values[i], _parserCulture), i);
            }
            return array;
        }
    }
}
