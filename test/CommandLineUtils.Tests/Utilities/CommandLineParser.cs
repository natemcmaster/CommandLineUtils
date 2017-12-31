// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using McMaster.Extensions.CommandLineUtils.Internal;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Utilities for parsing a command line application
    /// </summary>
    internal static class CommandLineParser
    {
        public static T ParseArgs<T>(params string[] args)
            where T : class, new()
        {
            var applicationBuilder = new ReflectionAppBuilder<T>();
            var context = new DefaultCommandLineContext(NullConsole.Singleton, Directory.GetCurrentDirectory(), args);
            return (T)applicationBuilder.Bind(context).ParentTarget;
        }
    }
}
