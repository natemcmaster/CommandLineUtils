// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    internal class BooleanValidator : IValidator
    {
        private readonly ValidationResult _result;

        public static IValidator True { get; } = new BooleanValidator(true);
        public static IValidator False { get; } = new BooleanValidator(false);

        private BooleanValidator(bool valid)
        {
            _result = valid ? ValidationResult.Success : new ValidationResult("Value was false");
        }

        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            => _result;

        public ValidationResult GetValidationResult(CommandArgument argument, ValidationContext context)
            => _result;
    }
}
