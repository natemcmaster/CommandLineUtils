// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Extension methods for adding validation rules to options and arguments.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Indicates the option is required.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="allowEmptyStrings">Indicates whether an empty string is allowed.</param>
        /// <param name="errorMessage">The custom error message to display. See also <seealso cref="ValidationAttribute.ErrorMessage"/>.</param>
        /// <returns>The option.</returns>
        public static CommandOption IsRequired(this CommandOption option, bool allowEmptyStrings = false, string errorMessage = null)
        {
            var attribute = GetValidationAttr<RequiredAttribute>(errorMessage);
            attribute.AllowEmptyStrings = allowEmptyStrings;
            option.Validators.Add(new AttributeValidator(attribute));
            return option;
        }

        /// <summary>
        /// Indicates the argument is required.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="allowEmptyStrings">Indicates whether an empty string is allowed.</param>
        /// <param name="errorMessage">The custom error message to display. See also <seealso cref="ValidationAttribute.ErrorMessage"/>.</param>
        /// <returns>The argument.</returns>
        public static CommandArgument IsRequired(this CommandArgument argument, bool allowEmptyStrings = false, string errorMessage = null)
        {
            var attribute = GetValidationAttr<RequiredAttribute>(errorMessage);
            attribute.AllowEmptyStrings = allowEmptyStrings;
            argument.Validators.Add(new AttributeValidator(attribute));
            return argument;
        }

        private static T GetValidationAttr<T>(string errorMessage)
            where T : ValidationAttribute, new()
        {
            var attribute = new T();
            if (errorMessage != null)
            {
                attribute.ErrorMessage = errorMessage;
            }
            return attribute;
        }
    }
}
