// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Specifies that the data must be an already existing file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilePathExistsAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes an instance of <see cref="FilePathExistsAttribute"/>.
        /// </summary>
        public FilePathExistsAttribute()
            // default error message
            : base("The file path '{0}' does not exist.")
        {
        }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(value is string path) || path.Length == 0 || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return new ValidationResult(FormatErrorMessage(value as string));
            }

            if (!Path.IsPathRooted(path)
                && validationContext.GetService(typeof(CommandLineContext)) is CommandLineContext context)
            {
                path = Path.Combine(context.WorkingDirectory, path);
            }

            if (!File.Exists(path))
            {
                return new ValidationResult(FormatErrorMessage(value as string));
            }

            return ValidationResult.Success;
        }
    }
}
