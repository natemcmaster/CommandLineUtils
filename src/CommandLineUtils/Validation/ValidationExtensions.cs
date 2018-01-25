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
        public static CommandOption Accepts(this CommandOption option, Action<IOptionValidationBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = new ValidationBuilder(option);
            configure(builder);
            return option;
        }

        /// <summary>
        /// Specifies a set of rules used to determine if input is valid.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="configure">A function to configure rules on the validation builder.</param>
        /// <returns>The argument.</returns>
        public static CommandArgument Accepts(this CommandArgument argument, Action<IArgumentValidationBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = new ValidationBuilder(argument);
            configure(builder);
            return argument;
        }

        /// <summary>
        /// Creates a builder for specifying a set of rules used to determine if input is valid.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>The builder.</returns>
        public static IOptionValidationBuilder Accepts(this CommandOption option)
            => new ValidationBuilder(option);

        /// <summary>
        /// Creates a builder for specifying a set of rules used to determine if input is valid.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The builder.</returns>
        public static IArgumentValidationBuilder Accepts(this CommandArgument argument)
            => new ValidationBuilder(argument);

        /// <summary>
        /// <para>
        /// Specifies that values must be one of the values in a given set.
        /// </para>
        /// <para>
        /// By default, value comparison is case-sensitive. To make matches case-insensitive, use <see cref="Values(IValidationBuilder, bool, string[])"/>.
        /// </para>
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="allowedValues">Allowed values.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder Values(this IValidationBuilder builder, params string[] allowedValues)
            => builder.Values(ignoreCase: false, allowedValues: allowedValues);

        /// <summary>
        /// Specifies that values must be one of the values in a given set.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="ignoreCase">Ignore case when comparing inputs to <paramref name="allowedValues"/>.</param>
        /// <param name="allowedValues">Allowed values.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder Values(this IValidationBuilder builder, bool ignoreCase, params string[] allowedValues)
        {
            var comparer = ignoreCase
                ? StringComparison.CurrentCultureIgnoreCase
                : StringComparison.CurrentCulture;
            return builder.Values(comparer, allowedValues);
        }

        /// <summary>
        /// Specifies that values must be one of the values in a given set.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="comparer">The comparer used to determine if values match.</param>
        /// <param name="allowedValues">Allowed values.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder Values(this IValidationBuilder builder, StringComparison comparer, params string[] allowedValues)
        {
            return builder.Satisfies<ValuesAttribute>(ctorArgs: new object[] { comparer, allowedValues });
        }

        /// <summary>
        /// Specifies that values must be a valid email address.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder EmailAddress(this IValidationBuilder builder, string errorMessage = null)
            => builder.Satisfies<EmailAddressAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a path to a file that already exists.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder ExistingFile(this IValidationBuilder builder, string errorMessage = null)
            => builder.Satisfies<FileExistsAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a path to a directory that already exists.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder ExistingDirectory(this IValidationBuilder builder, string errorMessage = null)
            => builder.Satisfies<DirectoryExistsAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a valid file path or directory, and the file path must already exist.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder ExistingFileOrDirectory(this IValidationBuilder builder, string errorMessage = null)
            => builder.Satisfies<FileOrDirectoryExistsAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be legal file paths.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder LegalFilePath(this IValidationBuilder builder, string errorMessage = null)
            => builder.Satisfies<LegalFilePathAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a string at least <paramref name="length"/> characters long.
        /// </summary>
        /// <param name="builder">The builder</param>
        /// <param name="length">The minimum length.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder MinLength(this IValidationBuilder builder, int length, string errorMessage = null)
            => builder.Satisfies<MinLengthAttribute>(errorMessage, length);

        /// <summary>
        /// Specifies that values must be a string no more than <paramref name="length"/> characters long.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="length">The maximum length.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder MaxLength(this IValidationBuilder builder, int length, string errorMessage = null)
            => builder.Satisfies<MaxLengthAttribute>(errorMessage, length);

        /// <summary>
        /// Specifies that values must match a regular expression.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="pattern">The regular expression.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder RegularExpression(this IValidationBuilder builder, string pattern, string errorMessage = null)
            => builder.Satisfies<RegularExpressionAttribute>(errorMessage, pattern);

        /// <summary>
        /// Specifies that values must satisfy the requirements of the validation attribute of type <typeparamref name="TAttribute"/>.
        /// </summary>
        /// <typeparam name="TAttribute">The validation attribute type.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="ctorArgs">Constructor arguments for <typeparamref name="TAttribute"/>.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder Satisfies<TAttribute>(this IValidationBuilder builder, string errorMessage = null, params object[] ctorArgs)
            where TAttribute : ValidationAttribute
        {
            var attribute = GetValidationAttr<TAttribute>(errorMessage, ctorArgs);
            builder.Use(new AttributeValidator(attribute));
            return builder;
        }

        private static T GetValidationAttr<T>(string errorMessage, object[] ctorArgs = null)
            where T : ValidationAttribute
        {
            var attribute = (T)Activator.CreateInstance(typeof(T), ctorArgs ?? new object[0]);
            if (errorMessage != null)
            {
                attribute.ErrorMessage = errorMessage;
            }
            return attribute;
        }
    }
}
