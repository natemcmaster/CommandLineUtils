// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class CollectionParserProvider
    {
        private CollectionParserProvider()
        { }

        public static CollectionParserProvider Default { get; } = new CollectionParserProvider();

        public ICollectionParser? GetParser(Type type, ValueParserProvider valueParsers)
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
#pragma warning disable CS8604 // Possible null reference argument.
                var elementParser = valueParsers.GetParser(elementType);
#pragma warning restore CS8604 // Possible null reference argument.

                return new ArrayParser(elementType, elementParser, valueParsers.ParseCulture);
            }

            if (type.IsGenericType)
            {
                var typeDef = type.GetGenericTypeDefinition();
                var elementType = type.GetGenericArguments().First();
                var elementParser = valueParsers.GetParser(elementType);

                if (typeof(IList<>) == typeDef
                    || typeof(IEnumerable<>) == typeDef
                    || typeof(ICollection<>) == typeDef
                    || typeof(IReadOnlyCollection<>) == typeDef
                    || typeof(IReadOnlyList<>) == typeDef
                    || typeof(List<>) == typeDef)
                {
                    return new ListParser(elementType, elementParser, valueParsers.ParseCulture);
                }

                if (typeof(ISet<>) == typeDef
                  || typeof(HashSet<>) == typeDef)
                {
                    return new HashSetParser(elementType, elementParser, valueParsers.ParseCulture);
                }
            }

            return null;
        }
    }
}
