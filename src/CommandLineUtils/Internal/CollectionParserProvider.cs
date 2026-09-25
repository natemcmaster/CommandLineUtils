// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class CollectionParserProvider
    {
        private CollectionParserProvider()
        { }

        public static CollectionParserProvider Default { get; } = new CollectionParserProvider();

        [UnconditionalSuppressMessage("Trimming", "IL2072",
            Justification = "type is annotated with DynamicallyAccessedMemberTypes.All, so its element/generic " +
                "argument type (an array element or the sole type argument of a single-generic-parameter " +
                "collection interface/List<>) is reachable through it, even though Type.GetElementType()/" +
                "GetGenericArguments() do not themselves carry a matching annotation on their return value.")]
        public ICollectionParser? GetParser([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, ValueParserProvider valueParsers)
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
