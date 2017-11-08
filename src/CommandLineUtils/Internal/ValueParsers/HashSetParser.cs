// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.ValueParsers
{
    internal class HashSetParser : ICollectionParser
    {
        private readonly IValueParser _elementParser;
        private readonly Type _listType;
        private readonly MethodInfo _addMethod;

        public HashSetParser(Type elementType, IValueParser elementParser)
        {
            _elementParser = elementParser;
            _listType = typeof(HashSet<>).MakeGenericType(elementType);
            _addMethod = _listType.GetRuntimeMethod("Add", new[] { elementType });
        }

        public object Parse(string argName, IReadOnlyList<string> values)
        {
            var set = Activator.CreateInstance(_listType, Constants.EmptyArray);
            for (var i = 0; i < values.Count; i++)
            {
                _addMethod.Invoke(set, new object[] { _elementParser.Parse(argName, values[i]) });
            }
            return set;
        }
    }
}
