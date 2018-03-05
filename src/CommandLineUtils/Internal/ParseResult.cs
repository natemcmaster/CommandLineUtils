// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class ParseResult
    {
        public CommandLineApplication SelectedCommand { get; set; }
        public CommandLineApplication InitialCommand { get; set; }
        public ValidationResult ValidationResult { get; set; }
    }
}
