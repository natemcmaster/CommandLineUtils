// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    internal class ThrowingValidator : IValidator
    {
        public static IValidator Singleton { get; } = new ThrowingValidator();

        private ThrowingValidator()
        { }

        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            throw new InvalidOperationException();
        }

        public ValidationResult GetValidationResult(CommandArgument argument, ValidationContext context)
        {
            throw new InvalidOperationException();
        }
    }
}
