// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Extension methods for adding validation rules to options and arguments.
    /// </summary>
    public static class ValidationExtensions
    {
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Indicates the option is required.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="allowEmptyStrings">Indicates whether an empty string is allowed.</param>
        /// <param name="errorMessage">The custom error message to display. See also: <see cref="ValidationAttribute.ErrorMessage"/>.</param>
        /// <returns>The option.</returns>
        public static CommandOption IsRequired(this CommandOption option, bool allowEmptyStrings = false, string? errorMessage = null)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            var attribute = GetValidationAttr<RequiredAttribute>(errorMessage);
            attribute.AllowEmptyStrings = allowEmptyStrings;
            option.Validators.Add(new AttributeValidator(attribute));
            return option;
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Indicates the option is required.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="allowEmptyStrings">Indicates whether an empty string is allowed.</param>
        /// <param name="errorMessage">The custom error message to display. See also: <see cref="ValidationAttribute.ErrorMessage"/>.</param>
        /// <returns>The option.</returns>
        public static CommandOption<T> IsRequired<T>(this CommandOption<T> option, bool allowEmptyStrings = false,
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            string? errorMessage = null)
        {

            IsRequired((CommandOption)option, allowEmptyStrings, errorMessage);
            return option;
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Indicates the argument is required.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="allowEmptyStrings">Indicates whether an empty string is allowed.</param>
        /// <param name="errorMessage">The custom error message to display. See also: <see cref="ValidationAttribute.ErrorMessage"/>.</param>
        /// <returns>The argument.</returns>
        public static CommandArgument IsRequired(this CommandArgument argument, bool allowEmptyStrings = false, string? errorMessage = null)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            var attribute = GetValidationAttr<RequiredAttribute>(errorMessage);
            attribute.AllowEmptyStrings = allowEmptyStrings;
            argument.Validators.Add(new AttributeValidator(attribute));
            return argument;
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Indicates the argument is required.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="allowEmptyStrings">Indicates whether an empty string is allowed.</param>
        /// <param name="errorMessage">The custom error message to display. See also: <see cref="ValidationAttribute.ErrorMessage"/>.</param>
        /// <returns>The argument.</returns>
        public static CommandArgument<T> IsRequired<T>(this CommandArgument<T> argument, bool allowEmptyStrings = false, string? errorMessage = null)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            IsRequired((CommandArgument)argument, allowEmptyStrings, errorMessage);
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
        /// Specifies a set of rules used to determine if input is valid.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="configure">A function to configure rules on the validation builder.</param>
        /// <returns>The option.</returns>
        public static CommandOption<T> Accepts<T>(this CommandOption<T> option, Action<IOptionValidationBuilder<T>> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = new ValidationBuilder<T>(option);
            configure(builder);
            return option;
        }

        /// <summary>
        /// Specifies a set of rules used to determine if input is valid.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="configure">A function to configure rules on the validation builder.</param>
        /// <returns>The argument.</returns>
        public static CommandArgument<T> Accepts<T>(this CommandArgument<T> argument, Action<IArgumentValidationBuilder<T>> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = new ValidationBuilder<T>(argument);
            configure(builder);
            return argument;
        }

        /// <summary>
        /// Creates a builder for specifying a set of rules used to determine if input is valid.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>The builder.</returns>
        public static IOptionValidationBuilder<T> Accepts<T>(this CommandOption<T> option)
            => new ValidationBuilder<T>(option);

        /// <summary>
        /// Creates a builder for specifying a set of rules used to determine if input is valid.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The builder.</returns>
        public static IArgumentValidationBuilder<T> Accepts<T>(this CommandArgument<T> argument)
            => new ValidationBuilder<T>(argument);

        /// <summary>
        /// <para>
        /// Specifies that values must be one of the values in a given set.
        /// </para>
        /// <para>
        /// By default, value comparison is case-sensitive. To make matches case-insensitive, set <paramref name="ignoreCase"/> to <c>true</c>.
        /// </para>
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="ignoreCase">Ignore case when parsing enums.</param>
        /// <exception cref="ArgumentException">When <typeparamref name="TEnum"/> is not an enum.</exception>
        /// <returns>The builder.</returns>
        public static IValidationBuilder Enum<TEnum>(this IValidationBuilder builder, bool ignoreCase = false)
            where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new ArgumentException("Type parameter T must be an enum.");
            }

            var comparer = ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return builder.Values(comparer, System.Enum.GetNames(typeof(TEnum)));
        }

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
            return builder.Satisfies<AllowedValuesAttribute>(ctorArgs: new object[] { comparer, allowedValues });
        }

        /// <summary>
        /// Specifies that values must be a valid email address.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder EmailAddress(this IValidationBuilder builder, string? errorMessage = null)
            => builder.Satisfies<EmailAddressAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a path to a file that already exists.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder ExistingFile(this IValidationBuilder builder, string? errorMessage = null)
            => builder.Satisfies<FileExistsAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a path to a file that does not already exist.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder NonExistingFile(this IValidationBuilder builder, string? errorMessage = null)
            => builder.Satisfies<FileNotExistsAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a path to a directory that already exists.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder ExistingDirectory(this IValidationBuilder builder, string? errorMessage = null)
            => builder.Satisfies<DirectoryExistsAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a path to a directory that does not already exist.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder NonExistingDirectory(this IValidationBuilder builder, string? errorMessage = null)
            => builder.Satisfies<DirectoryNotExistsAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a valid file path or directory, and the file path must already exist.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder ExistingFileOrDirectory(this IValidationBuilder builder, string? errorMessage = null)
            => builder.Satisfies<FileOrDirectoryExistsAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a valid file path or directory, and the file path must not already exist.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder NonExistingFileOrDirectory(this IValidationBuilder builder, string? errorMessage = null)
            => builder.Satisfies<FileOrDirectoryNotExistsAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be legal file paths.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder LegalFilePath(this IValidationBuilder builder, string? errorMessage = null)
            => builder.Satisfies<LegalFilePathAttribute>(errorMessage);

        /// <summary>
        /// Specifies that values must be a string at least <paramref name="length"/> characters long.
        /// </summary>
        /// <param name="builder">The builder</param>
        /// <param name="length">The minimum length.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder MinLength(this IValidationBuilder builder, int length, string? errorMessage = null)
            => builder.Satisfies<MinLengthAttribute>(errorMessage, length);

        /// <summary>
        /// Specifies that values must be a string no more than <paramref name="length"/> characters long.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="length">The maximum length.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder MaxLength(this IValidationBuilder builder, int length, string? errorMessage = null)
            => builder.Satisfies<MaxLengthAttribute>(errorMessage, length);

        /// <summary>
        /// Specifies that values must match a regular expression.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="pattern">The regular expression.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder RegularExpression(this IValidationBuilder builder, string pattern, string? errorMessage = null)
            => builder.Satisfies<RegularExpressionAttribute>(errorMessage, pattern);

        /// <summary>
        /// Specifies that values must satisfy the requirements of the validation attribute of type <typeparamref name="TAttribute"/>.
        /// </summary>
        /// <typeparam name="TAttribute">The validation attribute type.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="ctorArgs">Constructor arguments for <typeparamref name="TAttribute"/>.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder Satisfies<TAttribute>(this IValidationBuilder builder, string? errorMessage = null, params object[] ctorArgs)
            where TAttribute : ValidationAttribute
        {
            var attribute = GetValidationAttr<TAttribute>(errorMessage, ctorArgs);
            builder.Use(new AttributeValidator(attribute));
            return builder;
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Specifies that values must be in a given range.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="minimum">The minimum allowed value.</param>
        /// <param name="maximum">The maximum allowed value.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder<int> Range(this IValidationBuilder<int> builder, int minimum, int maximum, string? errorMessage = null)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            var attribute = GetValidationAttr<RangeAttribute>(errorMessage, new object[] { minimum, maximum });
            builder.Use(new AttributeValidator(attribute));
            return builder;
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Specifies that values must be in a given range.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="minimum">The minimum allowed value.</param>
        /// <param name="maximum">The maximum allowed value.</param>
        /// <param name="errorMessage">A custom error message to display.</param>
        /// <returns>The builder.</returns>
        public static IValidationBuilder<double> Range(this IValidationBuilder<double> builder, double minimum, double maximum, string? errorMessage = null)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            var attribute = GetValidationAttr<RangeAttribute>(errorMessage, new object[] { minimum, maximum });
            builder.Use(new AttributeValidator(attribute));
            return builder;
        }

        /// <summary>
        /// Adds a validator that runs after parsing is complete and before command execution.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="validate">The callback. Return <see cref="ValidationResult.Success"/> if there is no error.</param>
        /// <returns></returns>
        public static CommandLineApplication OnValidate(this CommandLineApplication command, Func<ValidationContext, ValidationResult> validate)
        {
            command.Validators.Add(new DelegateValidator(validate));
            return command;
        }

        /// <summary>
        /// Adds a validator that runs after parsing is complete and before command execution.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="validate">The callback. Return <see cref="ValidationResult.Success"/> if there is no error.</param>
        /// <returns></returns>
        public static CommandArgument OnValidate(this CommandArgument argument, Func<ValidationContext, ValidationResult> validate)
        {
            argument.Validators.Add(new DelegateValidator(validate));
            return argument;
        }

        /// <summary>
        /// Adds a validator that runs after parsing is complete and before command execution.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="validate">The callback. Return <see cref="ValidationResult.Success"/> if there is no error.</param>
        /// <returns></returns>
        public static CommandOption OnValidate(this CommandOption option, Func<ValidationContext, ValidationResult> validate)
        {
            option.Validators.Add(new DelegateValidator(validate));
            return option;
        }

        private static T GetValidationAttr<T>(string? errorMessage, object[]? ctorArgs = null)
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
