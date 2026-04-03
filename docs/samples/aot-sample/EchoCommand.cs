// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;

namespace AotSample
{

    /// <summary>
    /// Command that demonstrates RemainingArguments support.
    /// </summary>
    [Command(Name = "echo", Description = "Echo remaining arguments", UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue)]
    public class EchoCommand
    {

        /// <summary>
        /// Reference to the parent command (set by convention).
        /// </summary>
        public Program? Parent { get; set; }

        /// <summary>
        /// All remaining arguments (set by convention).
        /// </summary>
        public string[]? RemainingArguments { get; set; }

        internal int OnExecute()
        {
            if (RemainingArguments == null || RemainingArguments.Length == 0)
            {
                Console.WriteLine("No arguments to echo.");
            }
            else
            {
                Console.WriteLine("Echoing arguments:");

                foreach (var arg in RemainingArguments)
                {
                    Console.WriteLine($"  - {arg}");
                }
            }

            if (Parent?.Verbose == true)
            {
                Console.WriteLine($"  (Total: {RemainingArguments?.Length ?? 0} arguments)");
            }

            return 0;
        }

    }

}
