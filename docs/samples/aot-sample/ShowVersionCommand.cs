// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;

namespace AotSample
{

    /// <summary>
    /// Command that demonstrates name inference - "ShowVersionCommand" becomes "show-version".
    /// Note: No explicit Name property in [Command] attribute.
    /// </summary>
    [Command(Description = "Show version info (name inferred from class)")]
    public class ShowVersionCommand
    {

        /// <summary>
        /// Reference to the parent command (set by convention).
        /// </summary>
        public Program? Parent { get; set; }

        internal int OnExecute()
        {
            Console.WriteLine("Version: 2.0.0");
            Console.WriteLine("This command name was inferred from class name 'ShowVersionCommand'");

            if (Parent?.Verbose == true)
            {
                Console.WriteLine("  (Verbose mode enabled via parent)");
            }

            return 0;
        }

    }

}
