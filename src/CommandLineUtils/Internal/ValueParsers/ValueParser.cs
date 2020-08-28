// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// Provides methods for creating <see cref="IValueParser{T}"/>
    /// boilerplate implementations.
    /// </summary>

    public static class ValueParser
    {
        /// <summary>
        /// Creates an <see cref="IValueParser"/> implementation for a type
        /// given a parsing function that receives an argument name, a value
        /// to parse and a culture to use for parsing.
        /// </summary>

        public static IValueParser Create(Type targetType, Func<string?, string?, CultureInfo, object> parser) =>
            new DelegatingValueParser(targetType, Create(parser));

        /// <summary>
        /// Creates an <see cref="IValueParser{T}"/> implementation given
        /// a parsing function that receives an argument name, a value to
        /// parse and a culture to use for parsing.
        /// </summary>

        public static IValueParser<T> Create<T>(Func<string?, string?, CultureInfo, T> parser) =>
            new DelegatingValueParser<T>(parser);

        /// <summary>
        /// Creates an <see cref="IValueParser{T}"/> implementation given
        /// a parsing function that receives an argument name, a value to
        /// parse, a culture to use for parsing and returns a tuple whose
        /// first element indicates whether parsing was successful and
        /// second element is the parsed value.
        /// </summary>

        public static IValueParser<T> Create<T>(Func<string, CultureInfo, (bool, T)> parser) =>
            Create(parser, (argName, value) => new FormatException($"Invalid value specified for {argName}. '{value}' is an invalid representation of {typeof(T).Name}."));

        /// <summary>
        /// Creates an <see cref="IValueParser{T}"/> implementation given
        /// a parsing function that receives an argument name, a value to
        /// parse, a culture to use for parsing and returns a tuple whose
        /// first element indicates whether parsing was successful and
        /// second element is the parsed value. An additional parameter
        /// specifies a function that returns the <see cref="FormatException"/>
        /// to throw, given the argument name and value, when parsing is
        /// unsuccessful.
        /// </summary>

        public static IValueParser<T> Create<T>(Func<string, CultureInfo, (bool, T)> parser, Func<string?, string?, FormatException> errorSelector)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (errorSelector == null) throw new ArgumentNullException(nameof(errorSelector));

            return Create((argName, value, culture) =>
            {
                if (value == null) return default!;

                var (parsed, result) = parser(value, culture);
                return parsed ? result : throw errorSelector(argName, value);
            });
        }

        private sealed class DelegatingValueParser<T> : IValueParser<T>
        {
            readonly Func<string?, string?, CultureInfo, T> _parser;

            public DelegatingValueParser(Func<string?, string?, CultureInfo, T> parser)
            {
                _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            }

            public Type TargetType => typeof(T);

            public T Parse(string? argName, string? value, CultureInfo culture) =>
                _parser(argName, value, culture);

            object? IValueParser.Parse(string? argName, string? value, CultureInfo culture) =>
                Parse(argName, value, culture);
        }

        private class DelegatingValueParser : IValueParser
        {
            private readonly IValueParser<object> _parser;

            public DelegatingValueParser(Type targetType, IValueParser<object> parser)
            {
                TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
                _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            }

            public Type TargetType { get; }

            public object? Parse(string? argName, string? value, CultureInfo culture) =>
                _parser.Parse(argName, value, culture);
        }
    }
}
