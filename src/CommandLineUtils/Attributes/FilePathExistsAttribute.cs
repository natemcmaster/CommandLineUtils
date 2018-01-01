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
            : this(FilePathType.Any)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="FilePathExistsAttribute"/>.
        /// </summary>
        /// <param name="filePathType">Acceptable file path types</param>
        public FilePathExistsAttribute(FilePathType filePathType)
            : base(GetDefaultErrorMessage(filePathType))
        {
            FilePathType = filePathType;
        }

        /// <summary>
        /// Acceptable file path types.
        /// </summary>
        public FilePathType FilePathType { get; private set; }

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

            if ((FilePathType & FilePathType.File) != 0 && File.Exists(path))
            {
                return ValidationResult.Success;
            }

            if ((FilePathType & FilePathType.Directory) != 0 && Directory.Exists(path))
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
