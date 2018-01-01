// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Specifies that a value must be a legal file path.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LegalFilePathAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes an instance of <see cref="LegalFilePathAttribute"/>.
        /// </summary>
        public LegalFilePathAttribute()
            : base("'{0}' is an invalid file path.")
        {

        }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string path)
            {
                try
                {
                    var info = new FileInfo(path);
                    return ValidationResult.Success;
                }
                catch
                {
                }
            }

            return new ValidationResult(FormatErrorMessage(value as string));
        }
    }
}
