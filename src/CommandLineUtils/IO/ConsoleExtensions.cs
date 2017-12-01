// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Helper methods for <see cref="IConsole"/>.
    /// </summary>
    public static class ConsoleExtensions
    {
        /// <summary>
        /// Writes a string followed by a line terminator.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>the console.</returns>
        public static IConsole WriteLine(this IConsole console, string value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Writes a string console output.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>the console.</returns>
        public static IConsole Write(this IConsole console, string value)
        {
            console.Out.Write(value);
            return console;
        }
    }
}
