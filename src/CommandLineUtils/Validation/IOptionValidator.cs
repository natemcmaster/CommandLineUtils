// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Provides validation for a <see cref="CommandOption"/>.
    /// </summary>
    public interface IOptionValidator
    {
        /// <summary>
        /// Validates the values specified for <see cref="CommandOption.Values"/>.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="context">The validation context.</param>
        /// <returns>The validation result. Returns <see cref="ValidationResult.Success"/> if the values pass validation.</returns>
        ValidationResult GetValidationResult(CommandOption option, ValidationContext context);
    }
}
