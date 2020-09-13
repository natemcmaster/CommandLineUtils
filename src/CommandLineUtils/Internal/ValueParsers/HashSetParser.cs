// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class HashSetParser : ICollectionParser
    {
        private readonly IValueParser _elementParser;
        private readonly Type _listType;
        private readonly MethodInfo _addMethod;
        private readonly CultureInfo _parserCulture;

        public HashSetParser(Type elementType, IValueParser elementParser, CultureInfo parserCulture)
        {
            _elementParser = elementParser;
            _listType = typeof(HashSet<>).MakeGenericType(elementType);
            _addMethod = _listType.GetRuntimeMethod("Add", new[] { elementType });
            _parserCulture = parserCulture;
        }

        public object Parse(string? argName, IReadOnlyList<string?> values)
        {
            var set = Activator.CreateInstance(_listType, Util.EmptyArray<object>());
            foreach (var t in values)
            {
                _addMethod.Invoke(set, new[] { _elementParser.Parse(argName, t, _parserCulture) });
            }
            return set;
        }
    }
}
