// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Handles validation via OnValidate method without reflection.
    /// </summary>
    public interface IValidateHandler
    {
        /// <summary>
        /// Invokes the OnValidate method on the model.
        /// </summary>
        /// <param name="model">The model instance.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="commandContext">The command line context.</param>
        /// <returns>The validation result, or null if validation passed.</returns>
        ValidationResult? Invoke(object model, ValidationContext validationContext, CommandLineContext commandContext);
    }

    /// <summary>
    /// Strongly-typed handler for validation via OnValidate method.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IValidateHandler<TModel> : IValidateHandler
        where TModel : class
    {
        /// <summary>
        /// Invokes the OnValidate method on the model.
        /// </summary>
        /// <param name="model">The model instance.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="commandContext">The command line context.</param>
        /// <returns>The validation result, or null if validation passed.</returns>
        ValidationResult? Invoke(TModel model, ValidationContext validationContext, CommandLineContext commandContext);
    }

    /// <summary>
    /// Handles validation errors via OnValidationError method without reflection.
    /// </summary>
    public interface IValidationErrorHandler
    {
        /// <summary>
        /// Invokes the OnValidationError method on the model.
        /// </summary>
        /// <param name="model">The model instance.</param>
        /// <param name="validationResult">The validation result containing errors.</param>
        /// <returns>The exit code.</returns>
        int Invoke(object model, ValidationResult validationResult);
    }
}
