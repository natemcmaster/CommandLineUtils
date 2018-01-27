// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Specifies that the data must be an already existing file or directory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FileOrDirectoryExistsAttribute : FilePathExistsAttributeBase
    {
        /// <summary>
        /// Initializes an instance of <see cref="FileOrDirectoryExistsAttribute"/>.
        /// </summary>
        public FileOrDirectoryExistsAttribute()
            : base(FilePathType.Any)
        {
        }
    }
}
