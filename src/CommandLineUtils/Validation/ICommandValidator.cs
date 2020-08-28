// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Provides validation on a command
    /// </summary>
    public interface ICommandValidator
    {
        /// <summary>
        /// Validates a command
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="context">The validation context.</param>
        /// <returns>The validation result. Returns <see cref="ValidationResult.Success"/> if the values pass validation.</returns>
        ValidationResult GetValidationResult(CommandLineApplication command, ValidationContext context);
    }
}
