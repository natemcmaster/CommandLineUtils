// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents one or many positional command line arguments.
    /// Arguments are parsed in the order in which <see cref="CommandLineApplication.Arguments"/> lists them.
    /// The raw value must be parsable into type <typeparamref name="T" />.
    /// </summary>
    /// <seealso cref="CommandOption"/>
    public class CommandArgument<T> : CommandArgument, IInternalCommandParamOfT
    {
        private readonly List<T> _parsedValues = new();
        private readonly IValueParser<T> _valueParser;
        private bool _hasBeenParsed;
        private bool _hasDefaultValue;
        private T? _defaultValue;

        /// <summary>
        /// Initializes a new instance of <see cref="CommandArgument{T}" />
        /// </summary>
        /// <param name="valueParser">The value parser.</param>
        public CommandArgument(IValueParser<T> valueParser)
        {
            _valueParser = valueParser ?? throw new ArgumentNullException(nameof(valueParser));
            UnderlyingType = typeof(T);
        }

        /// <summary>
        /// The parsed value.
        /// </summary>
#pragma warning disable CS8603 // Possible null reference return.
        public T ParsedValue => ParsedValues.FirstOrDefault();
#pragma warning restore CS8603 // Possible null reference return.

        /// <summary>
        /// All parsed values;
        /// </summary>
        public IReadOnlyList<T> ParsedValues
        {
            get
            {
                if (!_hasBeenParsed)
                {
                    ((IInternalCommandParamOfT)this).Parse(CultureInfo.CurrentCulture);
                }

                if (_parsedValues.Count == 0 && _hasDefaultValue && DefaultValue != null)
                {
                    return new T[] { DefaultValue };
                }

                return _parsedValues;
            }
        }

        /// <summary>
        /// The default value of the argument.
        /// </summary>
        public new T? DefaultValue
        {
            get => _defaultValue;
            set
            {
                _hasDefaultValue = value != null;
                _defaultValue = value;
                SetBaseDefaultValue(value);
            }
        }

        void IInternalCommandParamOfT.Parse(CultureInfo culture)
        {
            _hasBeenParsed = true;
            _parsedValues.Clear();
            foreach (var t in base._values)
            {
                _parsedValues.Add(_valueParser.Parse(Name, t, culture));
            }
        }

        void SetBaseDefaultValue(T? value)
        {
            if (!ReflectionHelper.IsSpecialValueTupleType(typeof(T), out _))
            {
                if (MultipleValues && value is IEnumerable<object> enumerable)
                {
                    base.DefaultValue = string.Join(", ", enumerable.Select(x => x?.ToString()));
                }
                else
                {
                    base.DefaultValue = value?.ToString();
                }
            }
        }

        /// <inheritdoc />
        public override void Reset()
        {
            _hasBeenParsed = false;
            _parsedValues.Clear();
            base.Reset();
        }
    }
}
