// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using McMaster.Extensions.CommandLineUtils.Tests;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Utilities for parsing a command line application
    /// </summary>
    internal static class CommandLineParser
    {
        public static T ParseArgs<T>(params string[] args)
            where T : class
            => ParseArgsImpl<T>(NullConsole.Singleton, args);

        public static T ParseArgs<T>(ITestOutputHelper output, params string[] args)
            where T : class => ParseArgsImpl<T>(new TestConsole(output), args);

        private static T ParseArgsImpl<T>(IConsole console, string[] args) where T : class
        {
            var app = new CommandLineApplication<T>(console, Directory.GetCurrentDirectory());
            app.Conventions.UseDefaultConventions();
            app.Parse(args);
            return app.Model;
        }
    }
}
