// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.ValueParsers;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class ValueTupleParserProvider
    {
        private ValueTupleParserProvider() { }
        public static ValueTupleParserProvider Default { get; } = new ValueTupleParserProvider();

        public ITupleValueParser GetParser(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return null;
            }
            var typeDef = typeInfo.GetGenericTypeDefinition();
            if (typeDef == typeof(Tuple<,>) && typeInfo.GenericTypeArguments[0] == typeof(bool))
            {
                var innerParser = ValueParserProvider.Default.GetParser(typeInfo.GenericTypeArguments[1]);
                if (innerParser == null)
                {
                    return null;
                }
                var parserType = typeof(TupleValueParser<>).MakeGenericType(typeInfo.GenericTypeArguments[1]);
                return (ITupleValueParser)Activator.CreateInstance(parserType, new object[] { innerParser });
            }

            if (typeDef == typeof(ValueTuple<,>) && typeInfo.GenericTypeArguments[0] == typeof(bool))
            {
                var innerParser = ValueParserProvider.Default.GetParser(typeInfo.GenericTypeArguments[1]);
                if (innerParser == null)
                {
                    return null;
                }
                var parserType = typeof(ValueTupleValueParser<>).MakeGenericType(typeInfo.GenericTypeArguments[1]);
                return (ITupleValueParser)Activator.CreateInstance(parserType, new object[] { innerParser });
            }

            return null;
        }
    }
}
