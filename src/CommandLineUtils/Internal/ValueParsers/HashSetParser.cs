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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public HashSetParser(Type elementType, IValueParser elementParser, CultureInfo parserCulture)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _elementParser = elementParser;
            _listType = typeof(HashSet<>).MakeGenericType(elementType);
#pragma warning disable CS8601 // Possible null reference assignment.
            _addMethod = _listType.GetRuntimeMethod("Add", new[] { elementType });
#pragma warning restore CS8601 // Possible null reference assignment.
            _parserCulture = parserCulture;
        }

        public object Parse(string? argName, IReadOnlyList<string?> values)
        {
            var set = Activator.CreateInstance(_listType, Array.Empty<object>());
            foreach (var t in values)
            {
                _addMethod.Invoke(set, new[] { _elementParser.Parse(argName, t, _parserCulture) });
            }
#pragma warning disable CS8603 // Possible null reference return.
            return set;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
