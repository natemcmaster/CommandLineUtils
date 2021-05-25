// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// An option that returns a constant <see cref="ParsedValue"/> when specified on the command-line
    /// </summary>
    /// <typeparam name="T">type of <see cref="ConstantValue"/></typeparam>
    public class ConstantValueOption<T> : CommandOption, IOption<T>, IInternalCommandParamOfT
    {
        private readonly List<T> _parsedValues = new List<T>();
        private readonly T _constantValue;
        private bool _hasBeenParsed;

        /// <summary>
        /// Initializes a new <see cref="ConstantValueOption{T}"/>
        /// </summary>
        /// <param name="template">The option template.</param>
        /// <param name="constantValue">The value to return is specified on the command line</param>
        /// <see cref="MappedOption{T}"/>
        public ConstantValueOption(string template, T constantValue) : base(template, CommandOptionType.NoValue)
        {
            _constantValue = constantValue;
            UnderlyingType = typeof(T);
        }

        internal ConstantValueOption(T constantValue) : base(CommandOptionType.NoValue)
        {
            _constantValue = constantValue;
            UnderlyingType = typeof(T);
        }

        /// <summary>
        /// The value that is returned for any usage of this option
        /// </summary>
        public T ConstantValue => _constantValue;

        /// <summary>
        /// The parsed value.
        /// </summary>
        public T ParsedValue => ParsedValues.FirstOrDefault();

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

                return _parsedValues;
            }
        }

        /// <summary>
        /// "Parse" the user-given values into the constant value
        /// </summary>
        void IInternalCommandParamOfT.Parse(CultureInfo culture)
        {
            _hasBeenParsed = true;
            _parsedValues.Clear();
            foreach (var t in base._values)
            {
                _parsedValues.Add(_constantValue);
            }
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            _hasBeenParsed = false;
            _parsedValues.Clear();
            base.Reset();
        }
    }
}
