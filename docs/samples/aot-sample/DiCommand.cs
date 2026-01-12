// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;

namespace AotSample
{

    /// <summary>
    /// Command that demonstrates constructor injection.
    /// </summary>
    [Command(Name = "di", Description = "Demonstrate DI constructor injection")]
    public class DiCommand
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Reference to the parent command (set by convention).
        /// </summary>
        public Program? Parent { get; set; }

        /// <summary>
        /// Constructor that accepts an ILogger via dependency injection.
        /// </summary>
        public DiCommand(ILogger logger)
        {
            _logger = logger;
        }

        [Option("-m|--message", Description = "Message to log")]
        public string Message { get; set; } = "Hello from DI!";

        internal int OnExecute()
        {
            _logger.Log(Message);

            if (Parent?.Verbose == true)
            {
                Console.WriteLine("  (Verbose mode enabled via parent)");
            }

            return 0;
        }

    }

}
