// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Internal
{
    internal class DefaultCommandLineContext : CommandLineContext
    {
        public DefaultCommandLineContext()
        {
        }

        public DefaultCommandLineContext(IConsole console)
        {
            Console = console;
        }

        public DefaultCommandLineContext(IConsole console, string workDir)
            : this(console)
        {
            if (!Path.IsPathRooted(workDir))
            {
                workDir = Path.GetFullPath(workDir);
            }

            WorkingDirectory = workDir;
        }

        public DefaultCommandLineContext(IConsole console, string workDir, string[] args)
            : this(console, workDir)
        {
            Arguments = args;
        }
    }
}
