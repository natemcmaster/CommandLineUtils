// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.Validation;

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

        /// <summary>
        /// Specifies a set of rules used to determine if input is valid.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="configure">A function to configure rules on the validation builder.</param>
        /// <returns>The option.</returns>
        public static CommandOption Accepts(this CommandOption option, Func<ValidatorChainBuilder, ValidatorChain> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = new ValidatorChainBuilder();
            var rule = configure.Invoke(builder);
            option.Validators.Add(rule);
            return option;
        }

        /// <summary>
        /// Specifies a set of rules used to determine if input is valid.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="configure">A function to configure rules on the validation builder.</param>
        /// <returns>The argument.</returns>
        public static CommandArgument Accepts(this CommandArgument argument, Func<ValidatorChainBuilder, ValidatorChain> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = new ValidatorChainBuilder();
            var rule = configure.Invoke(builder);
            argument.Validators.Add(rule);
            return argument;
        }

        /// <summary>
        /// Specifies that values must be a valid email address.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>An executor that can be used to chain additional rules.</returns>
        public static ValidatorChain IsEmailAddress(this ValidatorChainBuilder builder, string errorMessage = null)
        {
            var attribute = GetValidationAttr<EmailAddressAttribute>(errorMessage);
            return builder.Build(new AttributeValidator(attribute));
        }

        /// <summary>
        /// Specifies that values must be a valid file path and the file must already exist.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>An executor that can be used to chain additional rules.</returns>
        public static ValidatorChain IsExistingFile(this ValidatorChainBuilder builder, string errorMessage = null)
        {
            var attribute = GetValidationAttr<FilePathExistsAttribute>(errorMessage);
            return builder.Build(new AttributeValidator(attribute));
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
