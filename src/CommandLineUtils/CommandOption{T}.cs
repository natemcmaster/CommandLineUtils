// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents one or many command line option that is identified by flag proceeded by '-' or '--'.
    /// Options are not positional. Compare to <see cref="CommandArgument{T}"/>. The raw value must be
    /// parsable into type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The type of the option value(s)</typeparam>
    public class CommandOption<T> : CommandOption, IInternalCommandParamOfT
    {
        private readonly List<T> _parsedValues = new();
        private readonly IValueParser<T> _valueParser;
        private T? _defaultValue;
        private bool _hasDefaultValue;
        private bool _hasBeenParsed;

        /// <summary>
        /// Initializes a new instance of <see cref="CommandOption{T}" />
        /// </summary>
        /// <param name="valueParser">The parser use to convert values into type of T.</param>
        /// <param name="template">The option template.</param>
        /// <param name="optionType">The option type</param>
        public CommandOption(IValueParser<T> valueParser, string template, CommandOptionType optionType)
            : base(template, optionType)
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
                    return new[] { DefaultValue };
                }

                return _parsedValues;
            }
        }

        /// <summary>
        /// The default value of the option.
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
                _parsedValues.Add(_valueParser.Parse(LongName ?? ShortName ?? SymbolName, t, culture));
            }
        }

        private void SetBaseDefaultValue(T? value)
        {
            if (!ReflectionHelper.IsSpecialValueTupleType(typeof(T), out _))
            {
                if (OptionType == CommandOptionType.MultipleValue && value is IEnumerable<object> enumerable)
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
