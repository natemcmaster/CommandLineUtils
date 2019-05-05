// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Specifies that the data must not be an already existing directory, not a file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DirectoryNotExistsAttribute : FilePathNotExistsAttributeBase
    {
        /// <summary>
        /// Initializes an instance of <see cref="DirectoryNotExistsAttribute"/>.
        /// </summary>
        public DirectoryNotExistsAttribute()
            : base(FilePathType.Directory)
        {
        }
    }
}
