// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// A store of value parsers that are used to convert argument values from strings to types.
    /// </summary>
    public class ValueParserProvider
    {
        private readonly Dictionary<Type, IValueParser> _parsers = new Dictionary<Type, IValueParser>(10);

        internal ValueParserProvider()
        {
            this.AddRange(
                new IValueParser[]
                {
                    StringValueParser.Singleton,
                    BooleanValueParser.Singleton,
                    ByteValueParser.Singleton,
                    Int16ValueParser.Singleton,
                    Int32ValueParser.Singleton,
                    Int64ValueParser.Singleton,
                    UInt16ValueParser.Singleton,
                    UInt32ValueParser.Singleton,
                    UInt64ValueParser.Singleton,
                    FloatValueParser.Singleton,
                    DoubleValueParser.Singleton,
                });
        }

        internal IValueParser GetParser(Type type)
        {
            if (this._parsers.TryGetValue(type, out var parser))
            {
                return parser;
            }

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsEnum)
            {
                return new EnumParser(type);
            }

            if (ReflectionHelper.IsNullableType(typeInfo, out var wrappedType))
            {
                if (wrappedType.GetTypeInfo().IsEnum)
                {
                    return new NullableValueParser(new EnumParser(wrappedType));
                }

                if (this._parsers.TryGetValue(wrappedType, out parser))
                {
                    return new NullableValueParser(parser);
                }
            }

            return parser;
        }


        /// <summary>
        /// Add a new value parser to the provider.
        /// </summary>
        /// <param name="parser">An instance of the parser that is used to convert an argument from a string.</param>
        /// <exception cref="ArgumentException">
        /// A value parser with the same <see cref="IValueParser.TargetType"/> is already registered.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="parser"/> is null.</exception>
        public void Add(IValueParser parser)
        {
            this.SafeAdd(parser);
        }

        /// <summary>
        /// Add collection of a new value parsers to the provider.
        /// </summary>
        /// <param name="parsers">The collection whose parsers should be added.</param>
        /// <exception cref="ArgumentException">
        /// A value parser with the same <see cref="IValueParser.TargetType"/> is already registered.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="parsers"/> is null.</exception>
        public void AddRange(IEnumerable<IValueParser> parsers)
        {
            if (parsers == null)
            {
                throw new ArgumentNullException(nameof(parsers));
            }

            foreach (var parser in parsers)
            {
                this.SafeAdd(parser);
            }
        }

        /// <summary>
        /// Add a new value parser to the provider, or if a value provider already exists for 
        /// <see cref="IValueParser.TargetType"/> then replaces it with <paramref name="parser"/>.
        /// </summary>
        /// <param name="parser">An instance of the parser that is used to convert an argument from a string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parser"/> is null.</exception>
        public void AddOrReplace(IValueParser parser)
        {
            this.SafeAdd(parser, andReplace: true);
        }

        private void SafeAdd(IValueParser parser, bool andReplace = false)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            var targetType = parser.TargetType;

            if (targetType == null)
            {
                throw new ArgumentNullException(
                    nameof(IValueParser.TargetType),
                    "The value parser must have a target type set");
            }

            // strip nullable wrappers since we have a dedicated nullable value parser
            targetType = ReflectionHelper.IsNullableType(targetType.GetTypeInfo(), out var wrappedType)
                ? wrappedType
                : targetType;
            
            if (this._parsers.ContainsKey(targetType))
            {
                if (andReplace)
                {
                    this._parsers.Remove(targetType);
                }
                else
                {
                    throw new ArgumentException(
                        $"Value parser provider for type '{targetType}' already exists.");
                }
            }

            this._parsers.Add(targetType, parser);
        }
    }
}
