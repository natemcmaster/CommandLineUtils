// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace McMaster.Extensions.CommandLineUtils.HelpText
{
    /// <summary>
    /// Generates help text for a command line application.
    /// </summary>
    public interface IHelpTextGenerator
    {
        /// <summary>
        /// Generate help text for the application.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="output"></param>
        void Generate(CommandLineApplication application, TextWriter output);
    }
}
