// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils.ValueParsers
{
    internal class ArrayParser : ICollectionParser
    {
        private readonly Type _elementType;
        private readonly IValueParser _elementParser;

        public ArrayParser(Type elementType, IValueParser elementParser)
        {
            _elementType = elementType;
            _elementParser = elementParser;
        }

        public object Parse(string argName, IReadOnlyList<string> values)
        {
            var array = Array.CreateInstance(_elementType, values.Count);
            for (var i = 0; i < values.Count; i++)
            {
                array.SetValue(_elementParser.Parse(argName, values[i]), i);
            }
            return array;
        }
    }
}
