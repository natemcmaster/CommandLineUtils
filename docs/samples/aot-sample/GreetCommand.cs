// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;

namespace AotSample
{

    /// <summary>
    /// Command that demonstrates basic option and argument handling.
    /// </summary>
    [Command(Name = "greet", Description = "Greet someone")]
    public class GreetCommand
    {

        /// <summary>
        /// Reference to the parent command (set by convention).
        /// </summary>
        public Program? Parent { get; set; }

        [Argument(0, Name = "name", Description = "The name of the person to greet")]
        [Required]
        public string Name { get; set; } = "";

        [Option("-l|--loud", Description = "Use uppercase")]
        public bool Loud { get; set; }

        internal int OnExecute()
        {
            var greeting = $"Hello, {Name}!";

            if (Loud)
            {
                greeting = greeting.ToUpperInvariant();
            }

            Console.WriteLine(greeting);

            // Show parent's verbose setting if available
            if (Parent?.Verbose == true)
            {
                Console.WriteLine("  (Verbose mode enabled via parent)");
            }

            return 0;
        }

    }

}
