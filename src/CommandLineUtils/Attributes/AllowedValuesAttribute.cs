﻿// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// <para>
    /// Specifies a set of allowed values and a comparer used to determine if a value is in that set.
    /// </para>
    /// <para>
    /// By default, value comparison is case-sensitive. To ensure case matches exactly, set <see cref="IgnoreCase"/> to <c>false</c>.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AllowedValuesAttribute : ValidationAttribute
    {
        private readonly string[] _allowedValues;

        /// <summary>
        /// Initializes an instance of <see cref="AllowedValuesAttribute"/>.
        /// </summary>
        /// <param name="allowedValues"></param>
        public AllowedValuesAttribute(params string[] allowedValues)
            : this(StringComparison.CurrentCulture, allowedValues)
        {
        }

        internal ReadOnlyCollection<string> AllowedValues => Array.AsReadOnly(_allowedValues);

        /// <summary>
        /// Initializes an instance of <see cref="AllowedValuesAttribute"/>.
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="allowedValues"></param>
        public AllowedValuesAttribute(StringComparison comparer, params string[] allowedValues)
            : base(GetDefaultError(allowedValues))
        {
            _allowedValues = allowedValues ?? Array.Empty<string>();
            Comparer = comparer;
        }

        private static string GetDefaultError(string[] allowedValues)
            => "Invalid value '{0}'. Allowed values are: " + string.Join(", ", allowedValues);

        /// <summary>
        /// The comparison method used.
        /// </summary>
        public StringComparison Comparer { get; set; }

        /// <summary>
        /// Comparison between values and allowed values should ignore case.
        /// </summary>
        public bool IgnoreCase
        {
            get => Comparer is StringComparison.CurrentCultureIgnoreCase
                    or StringComparison.InvariantCultureIgnoreCase
                    or StringComparison.OrdinalIgnoreCase;
            set => Comparer = value
                    ? StringComparison.CurrentCultureIgnoreCase
                    : StringComparison.CurrentCulture;
        }

        /// <inheritdoc />
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str && _allowedValues.Any(t => str.Equals(t, Comparer)))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(FormatErrorMessage(value?.ToString() ?? string.Empty));
        }
    }
}
