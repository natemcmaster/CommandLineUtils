// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// A <see cref="MappedOption{T}"/> is a set of <see cref="ConstantValueOption{T}"/>s that provide a single value.
    /// You could e.g. use this to get an enum-value by specifying a mapping from multiple options to enum-values.
    /// </summary>
    /// <typeparam name="T">type of <see cref="ParsedValues"/>/target of mappings</typeparam>
    public class MappedOption<T> : IEnumerable<ConstantValueOption<T>>, IOption<T>
    {
        /// <summary>
        /// <see cref="CommandLineApplication"/> to which this option belongs
        /// </summary>
        protected readonly CommandLineApplication _commandLineApplication;
        private readonly List<ConstantValueOption<T>> _mappings;
        private readonly ICollection<IOptionValidator> _validators;

        /// <summary>
        /// Create a new set of mapped options
        /// </summary>
        /// <param name="commandLineApplication">the application this option is for</param>
        /// <param name="optionType">the option type</param>
        /// <exception cref="ArgumentException">if optionType is something other than <see cref="CommandOptionType.MultipleValue"/> or <see cref="CommandOptionType.SingleValue"/></exception>
        public MappedOption(CommandLineApplication commandLineApplication, CommandOptionType optionType)
        {
            if (optionType != CommandOptionType.MultipleValue && optionType != CommandOptionType.SingleValue)
            {
                throw new ArgumentException("Only MultipleValue or SingleValue is supported.", nameof(optionType));
            }

            _commandLineApplication = commandLineApplication;
            _mappings = new List<ConstantValueOption<T>>();
            _validators = new List<IOptionValidator>();
            OptionType = optionType;

            if (optionType == CommandOptionType.SingleValue)
            {
                _validators.Add(new SingleValueValidator());
            }
        }

        /// <summary>
        /// Get's all the mapped values as <see cref="ConstantValueOption{T}"/>
        /// </summary>
        public IReadOnlyList<ConstantValueOption<T>> Mappings => _mappings;

        /// <summary>
        /// Add mapping
        /// </summary>
        /// <param name="template">parameter-template e.g. <code>-a|--apple</code></param>
        /// <param name="value">value to use if the mapped parameter was specified</param>
        /// <returns><code>this</code></returns>
        public MappedOption<T> Map(string template, T value)
        {
            Add(template, value);
            return this;
        }

        /// <summary>
        /// Add mapping
        /// </summary>
        /// <param name="description">description for use in the help-text</param>
        /// <param name="template">parameter-template e.g. <code>-a|--apple</code></param>
        /// <param name="value">value to use if the mapped parameter was specified</param>
        /// <returns><code>this</code></returns>
        public MappedOption<T> Map(string description, string template, T value)
        {
            Add(description, template, value);
            return this;
        }

        internal ConstantValueOption<T> Add(T value)
        {
            var constantValueOption = new ConstantValueOption<T>(value);
            _mappings.Add(constantValueOption);
            _commandLineApplication.AddOption(constantValueOption);
            return constantValueOption;
        }

        /// <summary>
        /// Add mapping
        /// </summary>
        /// <param name="template">parameter-template e.g. <code>-a|--apple</code></param>
        /// <param name="value">value to use if the mapped parameter was specified</param>
        /// <returns>the mapping</returns>
        public ConstantValueOption<T> Add(string template, T value)
        {
            var constantValueOption = new ConstantValueOption<T>(template, value);
            _mappings.Add(constantValueOption);
            _commandLineApplication.AddOption(constantValueOption);
            return constantValueOption;
        }

        /// <summary>
        /// Add mapping
        /// </summary>
        /// <param name="description">description for use in the help-text</param>
        /// <param name="template">parameter-template e.g. <code>-a|--apple</code></param>
        /// <param name="value">value to use if the mapped parameter was specified</param>
        /// <returns>the mapping</returns>
        public ConstantValueOption<T> Add(string description, string template, T value)
        {
            var constantValueOption = new ConstantValueOption<T>(template, value)
            {
                Description = description
            };
            _mappings.Add(constantValueOption);
            _commandLineApplication.AddOption(constantValueOption);
            return constantValueOption;
        }

        /// <inheritdoc />
        public IReadOnlyList<string?> Values => _mappings.SelectMany(m => m.Values).ToList();

        /// <inheritdoc />
        public CommandOptionType OptionType { get; }

        /// <inheritdoc />
        public bool Inherited { get; set; }

        /// <inheritdoc />
        public string? Description { get; set; }

        /// <inheritdoc />
        public ICollection<IOptionValidator> Validators => _validators;

        /// <inheritdoc />
        public bool TryParse(string? value)
        {
            return false;
        }

        /// <inheritdoc />
        public bool HasValue() => _mappings.Any(m => m.HasValue());

        /// <inheritdoc />
        public string? Value()
        {
            return _mappings.Where(m => m.HasValue())
                .Select(m => m.Value())
                .FirstOrDefault();
        }

        void IOption.Reset()
        {
            foreach (var mapping in _mappings)
            {
                ((IOption)mapping).Reset();
            }
        }

        /// <summary>
        /// The typed returned in <see cref="ParsedValues"/>
        /// </summary>
        public Type UnderlyingType => typeof(T);

        private IEnumerable<T> MappedValues =>
            _mappings.Where(m => m.HasValue())
                .Select(m => m.ParsedValue);

        /// <inheritdoc />
        public T ParsedValue => MappedValues.FirstOrDefault();

        /// <inheritdoc />
        public IReadOnlyList<T> ParsedValues => MappedValues.ToList();

        /// <inheritdoc />
        public IEnumerator<ConstantValueOption<T>> GetEnumerator()
        {
            return _mappings.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
