// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents one or many positional command line arguments.
    /// Arguments are parsed in the order in which <see cref="CommandLineApplication.Arguments"/> lists them.
    /// </summary>
    /// <seealso cref="CommandOption"/>
    public class CommandArgument
    {
        private protected List<string?> _values = new();

        /// <summary>
        /// The name of the argument.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Determines if the argument appears in the generated help-text.
        /// </summary>
        public bool ShowInHelpText { get; set; } = true;

        /// <summary>
        /// A description of the argument.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// All values specified, if any.
        /// </summary>
        public IReadOnlyList<string?> Values
        {
            get
            {
                if (_values.Count == 0 && DefaultValue != null)
                {
                    return new List<string?> { DefaultValue };
                }
                return _values;
            }
        }

        /// <summary>
        /// Allow multiple values.
        /// </summary>
        public bool MultipleValues { get; set; }

        /// <summary>
        /// The first value from <see cref="Values"/>, if any, or <see cref="DefaultValue" />.
        /// </summary>
        public string? Value => Values.FirstOrDefault();

        /// <summary>
        /// The default value of the argument.
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// True when <see cref="Values"/> is not empty or when a <see cref="DefaultValue" /> is given.
        /// </summary>
        public bool HasValue => Values.Any();

        /// <summary>
        /// A collection of validators that execute before invoking <see cref="CommandLineApplication.OnExecute(Func{int})"/>.
        /// When validation fails, <see cref="CommandLineApplication.ValidationErrorHandler"/> is invoked.
        /// </summary>
        public ICollection<IArgumentValidator> Validators { get; } = new List<IArgumentValidator>();

        /// <summary>
        /// Defines the underlying type of the argument for the help-text-generator
        /// </summary>
        internal Type? UnderlyingType { get; set; }

        /// <summary>
        /// Try to add a value to this argument.
        /// </summary>
        /// <param name="value">The argument value to be added.</param>
        /// <returns>True if the value was accepted, false if the value cannot be added.</returns>
        public bool TryParse(string? value)
        {
            if (_values.Count == 1 && !MultipleValues)
            {
                return false;
            }

            _values.Add(value);
            return true;
        }

        /// <summary>
        /// Clear any parsed values from this argument.
        /// </summary>
        public virtual void Reset()
        {
            _values.Clear();
        }
    }
}
