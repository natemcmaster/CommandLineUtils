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
        /// Pauses the application for the debugger when '--debug' is passed in.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        public static void HandleDebugSwitch(ref string[] args)
        {
            if (args.Length > 0 && string.Equals("--debug", args[0], StringComparison.OrdinalIgnoreCase))
            {
                args = args.Skip(1).ToArray();
                Console.WriteLine("Waiting for debugger to attach.");
                Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");
                Debugger.Launch();
                while (Debugger.IsAttached)
                {
                    Thread.Sleep(TimeSpan.MaxValue);
                }
            }
        }
    }
}
