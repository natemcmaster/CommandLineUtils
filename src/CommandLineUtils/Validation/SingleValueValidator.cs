// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Validates that only a single mapping inside a <see cref="MappedOption{T}"/> was specified on the command-line
    /// </summary>
    public class SingleValueValidator : IValidator
    {
        private readonly string _errorMessage;

        /// <summary>
        /// Initializes a new <see cref="SingleValueValidator"/>
        /// </summary>
        /// <param name="errorMessage"></param>
        public SingleValueValidator(string errorMessage = "Only one value is supported.")
        {
            _errorMessage = errorMessage;
        }

        /// <inheritdoc />
        public ValidationResult GetValidationResult(IOption option, ValidationContext context)
        {
            if (option.Values.Count > 1)
            {
                if (option is IParseableOption parseableOption)
                {
                    return new ValidationResult(_errorMessage,
                        new[]
                        {
                            parseableOption.LongName
                            ?? parseableOption.ShortName
                            ?? parseableOption.SymbolName
                            ?? parseableOption.Description
                            ?? "Unnamed option"
                        });
                }

                return new ValidationResult(_errorMessage,
                    new[] { option.Description ?? "Unnamed option" });
            }

            return ValidationResult.Success!;
        }

        /// <inheritdoc />
        public ValidationResult GetValidationResult(CommandArgument argument, ValidationContext context)
        {
            if (argument.Values.Count > 1)
            {
                return new ValidationResult(_errorMessage,
                    new[] {argument.Name ?? argument.Description
                        ?? "Unnamed option"});
            }

            return ValidationResult.Success!;
        }
    }
}
