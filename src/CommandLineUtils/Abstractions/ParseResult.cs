// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// The result of parsing command line arguments.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Initializes <see cref="ParseResult"/>.
        /// </summary>
        /// <param name="selectedCommand">The command selected for execution.</param>
        public ParseResult(CommandLineApplication selectedCommand)
        {
            SelectedCommand = selectedCommand ?? throw new ArgumentNullException(nameof(selectedCommand));
        }

        /// <summary>
        /// The application or subcommand that matches the command line arguments.
        /// </summary>
        public CommandLineApplication SelectedCommand { get; set; }
    }
}
