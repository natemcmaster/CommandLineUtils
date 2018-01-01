// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Base type for attributes that check for existing files or directories.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class FilePathExistsAttributeBase : ValidationAttribute
    {
        private readonly FilePathType _filePathType;

        /// <summary>
        /// Initializes an instance of <see cref="FilePathExistsAttributeBase"/>.
        /// </summary>
        public FilePathExistsAttributeBase()
            : this(FilePathType.Any)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="FilePathExistsAttributeBase"/>.
        /// </summary>
        /// <param name="filePathType">Acceptable file path types</param>
        public FilePathExistsAttributeBase(FilePathType filePathType)
            : base(GetDefaultErrorMessage(filePathType))
        {
            _filePathType = filePathType;
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

            if ((_filePathType & FilePathType.File) != 0 && File.Exists(path))
            {
                return ValidationResult.Success;
            }

            if ((_filePathType & FilePathType.Directory) != 0 && Directory.Exists(path))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(FormatErrorMessage(value as string));
        }

        private static string GetDefaultErrorMessage(FilePathType filePathType)
        {
            if (filePathType == FilePathType.File)
            {
                return "The file '{0}' does not exist.";
            }

            if (filePathType == FilePathType.Directory)
            {
                return "The directory '{0}' does not exist.";
            }

            return "The file path '{0}' does not exist.";
        }
    }
}
