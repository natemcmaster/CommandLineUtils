// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// The result of parsing command line arguments.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// The application or subcommand that matches the command line arguments.
        /// </summary>
        public CommandLineApplication SelectedCommand { get; set; }

        /// <summary>
        /// The result of executing all valiation on selected options and arguments.
        /// </summary>
        public ValidationResult ValidationResult { get; set; }
    }
}
