// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Utilities for parsing a command line application
    /// </summary>
    public static class CommandLineParser
    {
        /// <summary>
        /// Creates an instance of <typeparamref name="T" /> by matching <paramref name="args" />
        /// with the properties on <typeparamref name="T" />. 
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />, 
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="args">The arguments to parse</param>
        /// <typeparam name="T">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="CommandParsingException">Thrown when arguments cannot be parsed correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The type with the arguments parsed and applied to it.</returns>
        public static T ParseArgs<T>(params string[] args)
            where T : class, new()
        {
            var applicationBuilder = new ReflectionAppBuilder<T>();
            return (T)applicationBuilder.Bind(args).Target;
        }
    }
}
