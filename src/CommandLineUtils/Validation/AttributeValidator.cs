// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// A validator that uses a <see cref="ValidationAttribute"/> to validate a command line option or argument.
    /// </summary>
    public class AttributeValidator : IValidator
    {
        private readonly ValidationAttribute _attribute;

        /// <summary>
        /// Initializes an instance of <see cref="AttributeValidator"/>.
        /// </summary>
        /// <param name="attribute"></param>
        public AttributeValidator(ValidationAttribute attribute)
        {
            _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        }

        /// <summary>
        /// Gets the validation result for a command line option.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            if (_attribute is RequiredAttribute && option.OptionType == CommandOptionType.NoValue)
            {
                if (option.HasValue())
                {
                    return ValidationResult.Success;
                }
            }

            return GetValidationResult(option.Values, context);
        }

        /// <summary>
        /// Gets the validation result for a command line argument.
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public ValidationResult GetValidationResult(CommandArgument argument, ValidationContext context)
            => GetValidationResult(argument.Values, context);

        private ValidationResult GetValidationResult(List<string> values, ValidationContext context)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (_attribute is RequiredAttribute && values.Count == 0)
            {
                return _attribute.GetValidationResult(null, context);
            }

            foreach (var value in values)
            {
                var result = _attribute.GetValidationResult(value, context);
                if (result != ValidationResult.Success)
                {
                    return result;
                }
            }

            return ValidationResult.Success;
        }
    }
}
