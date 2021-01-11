// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{

    internal class ListParser : ICollectionParser
    {
        private readonly IValueParser _elementParser;
        private readonly Type _listType;
        private readonly CultureInfo _parserCulture;

        public ListParser(Type elementType, IValueParser elementParser, CultureInfo parserCulture)
        {
            _elementParser = elementParser;
            _listType = typeof(List<>).MakeGenericType(elementType);
            _parserCulture = parserCulture;
        }

        public object Parse(string? argName, IReadOnlyList<string?> values)
        {
            var list = (IList)Activator.CreateInstance(_listType, new object[] { values.Count });
            foreach (var t in values)
            {
                list.Add(_elementParser.Parse(argName, t, _parserCulture));
            }
            return list;
        }
    }
}
