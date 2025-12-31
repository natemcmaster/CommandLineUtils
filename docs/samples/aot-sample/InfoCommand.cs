// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;

namespace AotSample
{

    /// <summary>
    /// Command that displays runtime information.
    /// </summary>
    [Command(Name = "info", Description = "Display runtime information")]
    public class InfoCommand
    {

        /// <summary>
        /// Reference to the parent command (set by convention).
        /// </summary>
        public Program? Parent { get; set; }

        internal int OnExecute()
        {
            Console.WriteLine("AOT Sample Application");
            Console.WriteLine($"  Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"  OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
            Console.WriteLine($"  Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");

            // Check if we have generated metadata
            var hasGenerated = McMaster.Extensions.CommandLineUtils.SourceGeneration
                .CommandMetadataRegistry.HasMetadata(typeof(Program));
            Console.WriteLine($"  Generated Metadata: {hasGenerated}");

            // Show parent's verbose setting if available
            if (Parent?.Verbose == true)
            {
                Console.WriteLine("  (Verbose mode enabled)");
            }

            return 0;
        }

    }

}
