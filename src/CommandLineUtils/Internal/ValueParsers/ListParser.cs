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
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var list = (IList)Activator.CreateInstance(_listType, new object[] { values.Count });
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            foreach (var t in values)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                list.Add(_elementParser.Parse(argName, t, _parserCulture));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
#pragma warning disable CS8603 // Possible null reference return.
            return list;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
