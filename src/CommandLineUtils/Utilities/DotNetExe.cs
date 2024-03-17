// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Utilities for finding the "dotnet.exe" file from the currently running .NET Core application.
    /// </summary>
    public static class DotNetExe
    {
        private const string FileName = "dotnet";

        static DotNetExe()
        {
            FullPath = TryFindDotNetExePath();
        }

        /// <summary>
        /// The full filepath to the .NET Core CLI executable.
        /// <para>
        /// May be <c>null</c> if the CLI cannot be found.
        /// </para>
        /// </summary>
        /// <returns>The path or null</returns>
        /// <seealso cref="FullPathOrDefault" />
        public static string? FullPath { get; }

        /// <summary>
        /// Finds the full filepath to the .NET Core CLI executable,
        /// or returns a string containing the default name of the .NET Core muxer ('dotnet').
        /// <returns>The path or a string named 'dotnet'</returns>
        /// </summary>
        public static string FullPathOrDefault()
            => FullPath ?? FileName;

        private static string? TryFindDotNetExePath()
        {
            var fileName = FileName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName += ".exe";
            }

            var mainModule = Process.GetCurrentProcess().MainModule;
            if (!string.IsNullOrEmpty(mainModule?.FileName)
                && Path.GetFileName(mainModule.FileName).Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                return mainModule.FileName;
            }

            // DOTNET_ROOT specifies the location of the .NET runtimes, if they are not installed in the default location.
            var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");

            if (string.IsNullOrEmpty(dotnetRoot))
            {
                // fall back to default location
                // https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables#dotnet_root-dotnet_rootx86
                dotnetRoot = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "C:\\Program Files\\dotnet" : "/usr/local/share/dotnet";
            }

            return Path.Combine(dotnetRoot, fileName);
        }
    }
}
