// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Implements a validator with an anonymous function
    /// </summary>
    public class DelegateValidator : ICommandValidator, IArgumentValidator, IOptionValidator
    {
        private readonly Func<ValidationContext, ValidationResult> _validator;

        /// <summary>
        /// Initializes an instance of <see cref="DelegateValidator"/>.
        /// </summary>
        /// <param name="validator"></param>
        public DelegateValidator(Func<ValidationContext, ValidationResult> validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        ValidationResult ICommandValidator.GetValidationResult(CommandLineApplication command, ValidationContext context)
            => _validator(context);

        ValidationResult IArgumentValidator.GetValidationResult(CommandArgument argument, ValidationContext context)
            => _validator(context);

        ValidationResult IOptionValidator.GetValidationResult(CommandOption option, ValidationContext context)
            => _validator(context);
    }
}
