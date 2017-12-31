// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Internal
{
    internal class DefaultCommandLineContext : CommandLineContext
    {
        public DefaultCommandLineContext(string[] args, string workDir, IConsole console)
        {
            Arguments = args;
            WorkingDirectory = workDir;
            Console = console;
        }
    }
}
