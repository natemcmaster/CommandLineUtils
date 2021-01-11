// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Base type for attributes that check for files or directories not existing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class FilePathNotExistsAttributeBase : ValidationAttribute
    {
        private readonly FilePathType _filePathType;

        /// <summary>
        /// Initializes an instance of <see cref="FilePathNotExistsAttributeBase"/>.
        /// </summary>
        /// <param name="filePathType">Acceptable file path types</param>
        internal FilePathNotExistsAttributeBase(FilePathType filePathType)
            : base(GetDefaultErrorMessage(filePathType))
        {
            _filePathType = filePathType;
        }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not string path || path.Length == 0 || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return new ValidationResult(FormatErrorMessage(value as string));
            }

            if (!Path.IsPathRooted(path)
                && validationContext.GetService(typeof(CommandLineContext)) is CommandLineContext context)
            {
                path = Path.Combine(context.WorkingDirectory, path);
            }

            if ((_filePathType == FilePathType.File) && !File.Exists(path))
            {
                return ValidationResult.Success;
            }

            if ((_filePathType == FilePathType.Directory) && !Directory.Exists(path))
            {
                return ValidationResult.Success;
            }

            if ((_filePathType == FilePathType.Any) && !File.Exists(path) && !Directory.Exists(path))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(FormatErrorMessage(value as string));
        }

        private static string GetDefaultErrorMessage(FilePathType filePathType)
        {
            return filePathType switch
            {
                FilePathType.File => "The file '{0}' already exists.",
                FilePathType.Directory => "The directory '{0}' already exists.",
                _ => "The file path '{0}' already exists."
            };
        }
    }
}
