// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// A store of value parsers that are used to convert argument values from strings to types.
    /// </summary>
    public class ValueParserProvider
    {
        private readonly Dictionary<Type, IValueParser> _parsers = new(10);
        private readonly TypeDescriptorValueParserFactory _defaultValueParserFactory = new();

        internal ValueParserProvider()
        {
            AddRange(
                new IValueParser[]
                {
                    StockValueParsers.String,
                    StockValueParsers.Boolean,
                    StockValueParsers.Byte,
                    StockValueParsers.Int16,
                    StockValueParsers.Int32,
                    StockValueParsers.Int64,
                    StockValueParsers.UInt16,
                    StockValueParsers.UInt32,
                    StockValueParsers.UInt64,
                    StockValueParsers.Float,
                    StockValueParsers.Double,
                    StockValueParsers.Uri,
                    StockValueParsers.DateTime,
                    StockValueParsers.DateTimeOffset,
                    StockValueParsers.TimeSpan
                });
        }

        /// <summary>
        /// Gets or sets the CultureInfo which is used to convert argument values to types.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="CultureInfo.CurrentCulture"/>.
        /// </remarks>
        public CultureInfo ParseCulture { get; set; } = CultureInfo.CurrentCulture;

        private static readonly MethodInfo s_getParserGeneric
            = typeof(ValueParserProvider)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(m => m.Name == nameof(GetParser) && m.IsGenericMethod);

        /// <summary>
        /// Returns a parser registered for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IValueParser GetParser(Type type)
        {
            var method = s_getParserGeneric.MakeGenericMethod(type);
            return (IValueParser)method.Invoke(this, Array.Empty<object>());
        }

        /// <summary>
        /// Returns a parser for the generic type T.
        /// </summary>
        /// <remarks>
        /// If parser is not registered, null is returned.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IValueParser<T>? GetParser<T>()
        {
            var parser = GetParserImpl<T>();
            return parser switch
            {
                null => null,
                IValueParser<T> retVal => retVal,
                _ => new GenericParserAdapter<T>(parser)
            };
        }

        internal IValueParser? GetParserImpl<T>()
        {
            var type = typeof(T);
            if (_parsers.TryGetValue(type, out var parser))
            {
                return parser;
            }

            if (type.IsEnum)
            {
                return EnumParser.Create(type);
            }

            if (_defaultValueParserFactory.TryGetParser<T>(out parser))
            {
                return parser;
            }

            if (ReflectionHelper.IsNullableType(type, out var wrappedType))
            {
                if (wrappedType.IsEnum)
                {
                    return new NullableValueParser(EnumParser.Create(wrappedType));
                }

                if (_parsers.TryGetValue(wrappedType, out parser))
                {
                    return new NullableValueParser(parser);
                }
            }

            if (ReflectionHelper.IsSpecialValueTupleType(type, out var wrappedType2))
            {
                var innerParser = GetParser(wrappedType2);
                if (innerParser == null)
                {
                    return null;
                }
                var method = typeof(ValueTupleValueParser).GetMethod(nameof(ValueTupleValueParser.Create)).MakeGenericMethod(type.GenericTypeArguments[1]);
                return (IValueParser)method.Invoke(null, new object[] { innerParser });
            }

            return null;
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
            SafeAdd(parser);
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
                SafeAdd(parser);
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
            SafeAdd(parser, andReplace: true);
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
            targetType = ReflectionHelper.IsNullableType(targetType, out var wrappedType) && wrappedType != null
                ? wrappedType
                : targetType;

            if (_parsers.ContainsKey(targetType))
            {
                if (andReplace)
                {
                    _parsers.Remove(targetType);
                }
                else
                {
                    throw new ArgumentException(
                        $"Value parser provider for type '{targetType}' already exists.");
                }
            }

            _parsers.Add(targetType, parser);
        }

        private sealed class GenericParserAdapter<T> : IValueParser<T>
        {
            private readonly IValueParser _inner;

            public GenericParserAdapter(IValueParser inner)
            {
                _inner = inner;
            }

            public Type TargetType => _inner.TargetType;

            public T Parse(string? argName, string? value, CultureInfo culture)
                => (T)_inner.Parse(argName, value, culture)!;

            object? IValueParser.Parse(string? argName, string? value, CultureInfo culture)
                => _inner.Parse(argName, value, culture);
        }
    }
}
