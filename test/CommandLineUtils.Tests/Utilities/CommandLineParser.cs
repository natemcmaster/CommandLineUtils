// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            return (T)applicationBuilder.Bind(NullConsole.Singleton, args).Target;
        }
    }
}