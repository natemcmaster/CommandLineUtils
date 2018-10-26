// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    /// <summary>
    ///     A DI container for storing command line arguments.
    /// </summary>
    internal class CommandLineState : CommandLineContext
    {
        public CommandLineState(string[] args)
        {
            Arguments = args;
            Console = PhysicalConsole.Singleton;
        }

        public int ExitCode { get; set; }
    }
}
