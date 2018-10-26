// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    /// <summary>
    ///     A DI container for storing command line arguments.
    /// </summary>
    /// <seealso
    ///     cref="Microsoft.Extensions.Hosting.HostBuilderExtensions.UseCli{T}(Microsoft.Extensions.Hosting.IHostBuilder, string[])" />
    internal class CommandLineState
    {
        /// <value>The command line arguments</value>
        public string[] Arguments { get; set; }

        public int ExitCode { get; set; }
    }
}
