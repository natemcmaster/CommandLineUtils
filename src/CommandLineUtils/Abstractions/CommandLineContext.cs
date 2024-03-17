// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// Contains information about the execution context of the command-line application.
    /// </summary>
    public abstract class CommandLineContext
    {
        private string[] _args = Array.Empty<string>();
        private string _workDir = Directory.GetCurrentDirectory();
        private IConsole _console = PhysicalConsole.Singleton;

        /// <summary>
        /// The arguments as provided in Program.Main.
        /// </summary>
        /// <remarks>
        /// Cannot be null.
        /// </remarks>
        public string[] Arguments
        {
            get => _args;
            protected set => _args = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The current working directory. Defaults to <see cref="Directory.GetCurrentDirectory" />
        /// </summary>
        /// <remarks>
        /// Cannot be null, and must be an absolute file path.
        /// </remarks>
        public string WorkingDirectory
        {
            get => _workDir;
            protected set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (!Path.IsPathRooted(value))
                {
                    throw new ArgumentException(Strings.PathMustNotBeRelative, nameof(value));
                }

                _workDir = value;
            }
        }

        /// <summary>
        /// The console.
        /// </summary>
        /// <remarks>
        /// Cannot be null.
        /// </remarks>
        public IConsole Console
        {
            get => _console;
            protected set => _console = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
