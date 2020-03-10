// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.Hosting.CommandLine;
using McMaster.Extensions.Hosting.CommandLine.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    ///     Extension methods for <see cref="IHostBuilder" /> support.
    /// </summary>
    /// <seealso href="https://github.com/natemcmaster/CommandLineUtils/issues/134">host support</seealso>
    public static class HostBuilderExtensions
    {
        /// <summary>
        ///     Runs an instance of <typeparamref name="TApp" /> using <see cref="CommandLineApplication" /> to provide
        ///     command line parsing on the given <paramref name="args" />.  This method should be the primary approach
        ///     taken for command line applications.
        /// </summary>
        /// <typeparam name="TApp">The type of the command line application implementation</typeparam>
        /// <param name="hostBuilder">This instance</param>
        /// <param name="args">The command line arguments</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task whose result is the exit code of the application</returns>
        public static async Task<int> RunCommandLineApplicationAsync<TApp>(
            this IHostBuilder hostBuilder, string[] args, CancellationToken cancellationToken = default)
            where TApp : class
        {
            var exceptionHandler = new StoreExceptionHandler();
            var state = new CommandLineState(args);
            hostBuilder.ConfigureServices(
                (context, services)
                    =>
                {
                    services
                        .TryAddSingleton<IUnhandledExceptionHandler>(exceptionHandler);
                    services
                        .AddSingleton<IHostLifetime, CommandLineLifetime>()
                        .TryAddSingleton(PhysicalConsole.Singleton);
                    services
                        .AddSingleton(provider =>
                        {
                            state.SetConsole(provider.GetService<IConsole>());
                            return state;
                        })
                        .AddSingleton<CommandLineContext>(state)
                        .AddSingleton<ICommandLineService, CommandLineService<TApp>>();
                });

            using var host = hostBuilder.Build();
            await host.RunAsync(cancellationToken);

            if (exceptionHandler.StoredException != null)
            {
                ExceptionDispatchInfo.Capture(exceptionHandler.StoredException).Throw();
            }

            return state.ExitCode;
        }

        /// <summary>
        ///     Runs an instance of <see cref="CommandLineApplication" /> using builder API to provide
        ///     command line parsing on the given <paramref name="args" />.
        /// </summary>
        /// <param name="hostBuilder">This instance</param>
        /// <param name="args">The command line arguments</param>
        /// <param name="configure">The delegate to configure the application</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task whose result is the exit code of the application</returns>
#pragma warning disable RS0026 // disable warning about optional parameter for CancellationToken
        public static async Task<int> RunCommandLineApplicationAsync(
#pragma warning restore RS0026
            this IHostBuilder hostBuilder,
            string[] args,
            Action<CommandLineApplication> configure,
            CancellationToken cancellationToken = default)
        {
            var exceptionHandler = new StoreExceptionHandler();
            var state = new CommandLineState(args);
            hostBuilder.ConfigureServices(
                (context, services)
                    =>
                {
                    services
                        .TryAddSingleton<IUnhandledExceptionHandler>(exceptionHandler);
                    services
                        .AddSingleton<IHostLifetime, CommandLineLifetime>()
                        .TryAddSingleton(PhysicalConsole.Singleton);
                    services
                        .AddSingleton(provider =>
                        {
                            state.SetConsole(provider.GetService<IConsole>());
                            return state;
                        })
                        .AddSingleton<CommandLineContext>(state)
                        .AddSingleton<ICommandLineService, CommandLineService>();
                    services
                    .AddSingleton(configure);
                });

            using var host = hostBuilder.Build();

            await host.RunAsync(cancellationToken);

            if (exceptionHandler.StoredException != null)
            {
                ExceptionDispatchInfo.Capture(exceptionHandler.StoredException).Throw();
            }

            return state.ExitCode;
        }
    }
}
