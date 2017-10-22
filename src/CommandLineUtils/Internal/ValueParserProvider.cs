// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.ValueParsers;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class ValueParserProvider
    {
        private Dictionary<Type, IValueParser> _parsers = new Dictionary<Type, IValueParser>
        {
            { typeof(string), StringValueParser.Singleton },
            { typeof(bool), BooleanValueParser.Singleton },
            { typeof(byte), ByteValueParser.Singleton },
            { typeof(short), Int16ValueParser.Singleton },
            { typeof(int), Int32ValueParser.Singleton },
            { typeof(long), Int64ValueParser.Singleton },
            { typeof(ushort), UInt16ValueParser.Singleton },
            { typeof(uint), UInt32ValueParser.Singleton },
            { typeof(ulong), UInt64ValueParser.Singleton },
        };

        private ValueParserProvider()
        { }

        public static ValueParserProvider Default { get; } = new ValueParserProvider();

        public IValueParser GetParser(Type type)
        {
            if (_parsers.TryGetValue(type, out var parser))
            {
                return parser;
            }

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var wrappedType = type.GetTypeInfo().GetGenericArguments().First();
                if (_parsers.TryGetValue(wrappedType, out parser))
                {
                    return new NullableValueParser(parser);
                }
            }

            return parser;
        }
    }
}
