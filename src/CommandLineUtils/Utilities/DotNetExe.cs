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
#if NET45
            fileName += ".exe";
#elif NETSTANDARD2_0 || NETSTANDARD2_1
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
#else
#error Update target frameworks
#endif
            var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            return !string.IsNullOrEmpty(dotnetRoot)
                ? Path.Combine(dotnetRoot, fileName)
                : null;
        }
    }
}
