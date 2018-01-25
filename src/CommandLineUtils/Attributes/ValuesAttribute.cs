// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

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
    public sealed class ValuesAttribute : ValidationAttribute
    {
        private readonly string[] _allowedValues;

        /// <summary>
        /// Initializes an instance of <see cref="ValuesAttribute"/>.
        /// </summary>
        /// <param name="allowedValues"></param>
        public ValuesAttribute(params string[] allowedValues)
            : this(StringComparison.CurrentCulture, allowedValues)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ValuesAttribute"/>.
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="allowedValues"></param>
        public ValuesAttribute(StringComparison comparer, params string[] allowedValues)
            : base(GetDefaultError(allowedValues))
        {
            _allowedValues = allowedValues ?? new string[0];
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
            get
            {
                return Comparer == StringComparison.CurrentCultureIgnoreCase
#if (NETSTANDARD2_0 || NET45)
                    || Comparer == StringComparison.InvariantCultureIgnoreCase
#elif NETSTANDARD1_6
#else
#error Target frameworks should be updated
#endif
                    || Comparer == StringComparison.OrdinalIgnoreCase;
            }
            set
            {
                Comparer = value
                    ? StringComparison.CurrentCultureIgnoreCase
                    : StringComparison.CurrentCulture;
            }
        }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string str)
            {
                for (var i = 0; i < _allowedValues.Length; i++)
                {
                    if (str.Equals(_allowedValues[i], Comparer))
                    {
                        return ValidationResult.Success;
                    }
                }
            }

            return new ValidationResult(FormatErrorMessage(value as string));
        }
    }
}
