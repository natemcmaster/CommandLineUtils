// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.ValueParsers;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class CollectionParserProvider
    {
        private CollectionParserProvider()
        { }

        public static CollectionParserProvider Default { get; } = new CollectionParserProvider();

        public ICollectionParser GetParser(Type type)
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var elementParser = ValueParserProvider.Default.GetParser(elementType);
                if (elementParser == null)
                {
                    return null;
                }

                return new ArrayParser(elementType, elementParser);
            }

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType)
            {
                var typeDef = type.GetGenericTypeDefinition();
                var elementType = typeInfo.GetGenericArguments().First();
                var elementParser = ValueParserProvider.Default.GetParser(elementType);

                if (typeof(IList<>) == typeDef
                    || typeof(IEnumerable<>) == typeDef
                    || typeof(ICollection<>) == typeDef
                    || typeof(IReadOnlyCollection<>) == typeDef
                    || typeof(IReadOnlyList<>) == typeDef
                    || typeof(List<>) == typeDef)
                {
                    return new ListParser(elementType, elementParser);
                }

                if (typeof(ISet<>) == typeDef
                  || typeof(HashSet<>) == typeDef)
                {
                    return new HashSetParser(elementType, elementParser);
                }
            }

            return null;
        }
    }
}
