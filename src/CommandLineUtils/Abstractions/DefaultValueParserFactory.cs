namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// A factory creating generic implementations of <see cref="IValueParser{T}"/>. The implementations are based
    /// on automatically located <see cref="TypeConverter"/> classes that are suitable for parsing.
    /// </summary>
    sealed class DefaultValueParserFactory
    {
        const int DefaultMaxCacheCapacity = 100;

        #region Private Fields

        private readonly int _maxCacheCapacity;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        readonly Dictionary<Type, IValueParser> _parsersByTargetType = new Dictionary<Type, IValueParser>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly HashSet<Type> _notSupportedTypes = new HashSet<Type>();

        #endregion

        public DefaultValueParserFactory(int maxCacheCapacity = DefaultMaxCacheCapacity)
        {
            _maxCacheCapacity = maxCacheCapacity;
            if (_maxCacheCapacity < 1) throw new ArgumentOutOfRangeException(nameof(maxCacheCapacity));
        }

        [DebuggerStepThrough]
        public bool TryGetParser<T>(out IValueParser<T> parser)
        {
            if (TryGetParser<T>(out IValueParser generalizedParser))
            {
                parser = (IValueParser<T>)generalizedParser;
                return true;
            }

            parser = null;
            return false;
        }

        public bool TryGetParser<T>(out IValueParser parser)
        {
            var targetType = typeof(T);
            if (_notSupportedTypes.Contains(targetType))
            {
                parser = null;
                return false;
            }

            if (_parsersByTargetType.TryGetValue(targetType, out parser))
            {
                Debug.Assert(targetType == parser.TargetType);
                return true;
            }

            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                if (_parsersByTargetType.Count >= _maxCacheCapacity)
                    _parsersByTargetType.Clear();
                parser = new TypeConverterValueParser<T>(targetType, converter);
                _parsersByTargetType[targetType] = parser;
                Debug.Assert(_parsersByTargetType.Count <= _maxCacheCapacity);
                return true;
            }

            parser = null;
            if (_notSupportedTypes.Count > _maxCacheCapacity)
                _notSupportedTypes.Clear();
            _notSupportedTypes.Add(targetType);
            Debug.Assert(_notSupportedTypes.Count <= _maxCacheCapacity);
            return false;
        }

        public IValueParser<T> GetParser<T>()
        {
            return TryGetParser<T>(out IValueParser<T> converter)
                ? converter
                : throw new NotSupportedException(
                    new StringBuilder($"No suitable type converter found for {typeof(T)}.")
                        .Append($" Make sure a type converter capable of parsing {typeof(string)} to {typeof(T)} exists and is discoverable.")
                        .Append($" Did you forget to annotate the target type with {typeof(TypeConverterAttribute)}?")
                        .ToString());
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

            public T Parse(string argName, string value, CultureInfo culture)
            {
                try
                {
                    culture ??= CultureInfo.InvariantCulture;
                    return (T)TypeConverter.ConvertFromString(null, culture, value);
                }
                catch (ArgumentException e)
                {
                    throw new FormatException(e.Message, e);
                }
            }

            object IValueParser.Parse(string argName, string value, CultureInfo culture) => Parse(argName, value, culture);
        }

    }
}
