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
        /// </summary>
        public static string FullPath { get; }

        /// <summary>
        /// Finds the full filepath to the .NET Core CLI executable,
        /// or returns a string containing the default name of the .NET Core muxer ('dotnet').
        /// </summary>
        /// <returns>The path or a string named 'dotnet'</returns>
        public static string FullPathOrDefault()
            => FullPath ?? FileName;

        private static string TryFindDotNetExePath()
        {
#if NET45
            return "dotnet.exe";
#elif (NETSTANDARD1_6 || NETSTANDARD2_0)
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

            // if Process.MainModule is not available or it does not equal "dotnet(.exe)?", fallback to navigating to the muxer
            // by using the location of the shared framework

            var fxDepsFile = AppContext.GetData("FX_DEPS_FILE") as string;

            if (string.IsNullOrEmpty(fxDepsFile))
            {
                return null;
            }

            var muxerDir = new FileInfo(fxDepsFile) // Microsoft.NETCore.App.deps.json
                .Directory? // (version)
                .Parent? // Microsoft.NETCore.App
                .Parent? // shared
                .Parent; // DOTNET_HOME

            if (muxerDir == null)
            {
                return null;
            }

            var muxer = Path.Combine(muxerDir.FullName, fileName);
            return File.Exists(muxer)
                ? muxer
                : null;
#endif
        }
    }
}
