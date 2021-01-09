// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.Hosting
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using McMaster.Extensions.Hosting.CommandLine.Internal;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for <see cref="IHost" /> support.
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Runs the app using the <see cref="CommandLineApplication" /> previously configured in
        /// <see cref="HostBuilderExtensions.UseCommandLineApplication{TApp}(IHostBuilder, string[], Action{CommandLineApplication{TApp}})"/>.
        /// </summary>
        /// <param name="host">A program abstraction.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        public static async Task<int> RunCommandLineApplicationAsync(this IHost host, CancellationToken cancellationToken = default)
        {
            var exceptionHandler = host.Services.GetService<StoreExceptionHandler>();
            var state = host.Services.GetRequiredService<CommandLineState>();

            await host.RunAsync(cancellationToken);

            if (exceptionHandler?.StoredException != null)
            {
                ExceptionDispatchInfo.Capture(exceptionHandler.StoredException).Throw();
            }

            return state.ExitCode;
        }
    }
}
