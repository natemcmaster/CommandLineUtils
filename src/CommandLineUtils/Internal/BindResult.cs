// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class BindResult
    {
        /// <summary>
        /// The command selected based on input args.
        /// </summary>
        public CommandLineApplication Command { get; set; }

        /// <summary>
        /// An instance of the object that matches <see cref="Command" />.
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// The top-level target type. The same as <see cref="Target" /> on single-level apps.
        /// </summary>
        public object ParentTarget { get; set; }

        /// <summary>
        /// Validation result, if any.
        /// </summary>
        public ValidationResult ValidationResult { get; set; }
    }
}
