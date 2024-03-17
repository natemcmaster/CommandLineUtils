// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// A factory creating generic implementations of <see cref="IValueParser{T}"/>. The implementations are based
    /// on automatically located <see cref="TypeConverter"/> classes that are suitable for parsing.
    /// </summary>
    internal class TypeDescriptorValueParserFactory
    {
        public bool TryGetParser<T>([NotNullWhen(true)] out IValueParser? parser)
        {
            var targetType = typeof(T);
            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                parser = new TypeConverterValueParser<T>(targetType, converter);
                return true;
            }

            parser = null;
            return false;
        }

        private sealed class TypeConverterValueParser<T> : IValueParser<T>
        {
            public TypeConverterValueParser(Type targetType, TypeConverter typeConverter)
            {
                TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
                TypeConverter = typeConverter ?? throw new ArgumentNullException(nameof(typeConverter));
            }

            public Type TargetType { get; }

            private TypeConverter TypeConverter { get; }

            public T Parse(string? argName, string? value, CultureInfo culture)
            {
                try
                {
                    culture ??= CultureInfo.InvariantCulture;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.
                    return (T)TypeConverter.ConvertFromString(null, culture, value);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
                catch (ArgumentException e)
                {
                    throw new FormatException(e.Message, e);
                }
            }

            object? IValueParser.Parse(string? argName, string? value, CultureInfo culture)
                => Parse(argName, value, culture);
        }
    }
}
