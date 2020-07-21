// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Specifies that the data must be an already existing directory, not a file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DirectoryExistsAttribute : FilePathExistsAttributeBase
    {
        /// <summary>
        /// Initializes an instance of <see cref="FileExistsAttribute"/>.
        /// </summary>
        public DirectoryExistsAttribute()
            : base(FilePathType.Directory)
        {
        }
    }
}
