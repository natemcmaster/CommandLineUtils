// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class UnrecognizedCommandParsingException : CommandParsingException
    {
        public UnrecognizedCommandParsingException(CommandLineApplication command, string message) : base(command, message)
        { }

        public string NearestMatch { get; set; }
    }
}
