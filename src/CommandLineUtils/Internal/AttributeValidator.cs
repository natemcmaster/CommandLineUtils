// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class AttributeValidator : IOptionValidator, IArgumentValidator
    {
        private readonly ValidationAttribute _attribute;

        public AttributeValidator(ValidationAttribute attribute)
        {
            _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        }

        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            => GetValidationResult(option.Values, context);

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
