// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// Represents file path types.
    /// </summary>
    [Flags]
    public enum FilePathType
    {
        /// <summary>
        /// A file path to a directory.
        /// </summary>
        Directory = 1 << 0,

        /// <summary>
        /// A file path to a file.
        /// </summary>
        File = 1 << 1,

        /// <summary>
        /// Any type of filepath.
        /// </summary>
        Any = Directory | File,
    }
}
