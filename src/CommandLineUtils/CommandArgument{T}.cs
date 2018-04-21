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
    /// Compare to <seealso cref="CommandOption"/>. The raw value must be
    /// parsable into type <typeparamref name="T" />
    /// </summary>
    public class CommandArgument<T> : CommandArgument, IInternalCommandParamOfT
    {
        private readonly List<T> _parsedValues = new List<T>();
        private readonly IValueParser<T> _valueParser;

        /// <summary>
        /// Initializes a new instance of <see cref="CommandArgument{T}" />
        /// </summary>
        /// <param name="valueParser">The value parser.</param>
        public CommandArgument(IValueParser<T> valueParser)
        {
            _valueParser = valueParser ?? throw new ArgumentNullException(nameof(valueParser));
        }

        /// <summary>
        /// The parsed value.
        /// </summary>
        public T ParsedValue => _parsedValues.FirstOrDefault();

        /// <summary>
        /// All parsed values;
        /// </summary>
        public IReadOnlyList<T> ParsedValues => _parsedValues;

        void IInternalCommandParamOfT.Parse(CultureInfo culture)
        {
            _parsedValues.Clear();
            for (int i = 0; i < Values.Count; i++)
            {
                _parsedValues.Add(_valueParser.Parse(Name, Values[i], culture));
            }
        }
    }
}
