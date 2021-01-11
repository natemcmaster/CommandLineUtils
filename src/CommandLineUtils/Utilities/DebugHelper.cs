// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Helps handle debug command-line arguments.
    /// </summary>
    public static class DebugHelper
    {
        /// <summary>
        /// Pauses the application until the debugger is attached when '--debug' is passed in as the first argument.
        /// <para>
        /// The pause times out at 30 seconds and continues execution.
        /// </para>
        /// </summary>
        /// <param name="args">The command line arguments</param>
        public static void HandleDebugSwitch(ref string[] args)
            => HandleDebugSwitch(ref args, 30);

        /// <summary>
        /// Pauses the application until the debugger is attached when '--debug' is passed in as the first argument,
        /// with a maximum wait time in seconds.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <param name="maxWaitSeconds">Maximum number of seconds to wait. Set to 0 or less for infinite waiting.</param>
        public static void HandleDebugSwitch(ref string[] args, int maxWaitSeconds)
        {
            if (args.Length > 0 && string.Equals("--debug", args[0], StringComparison.OrdinalIgnoreCase))
            {
                args = args.Skip(1).ToArray();
                if (Debugger.IsAttached)
                {
                    return;
                }

                Console.WriteLine("Waiting for debugger to attach.");
                Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");

                const int interval = 250;
                var maxWait = maxWaitSeconds * 1000 / interval;
                while (!Debugger.IsAttached && (maxWait > 0 || maxWaitSeconds <= 0))
                {
                    maxWait--;
                    Thread.Sleep(TimeSpan.FromMilliseconds(interval));
                }

                if (!Debugger.IsAttached)
                {
                    Console.WriteLine($"Timed out waiting for {maxWaitSeconds} seconds for debugger to attach. Continuing execution.");
                }
            }
        }
    }
}
