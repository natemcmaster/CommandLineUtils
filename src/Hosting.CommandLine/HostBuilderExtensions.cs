// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            this IHostBuilder hostBuilder,
            string[] args,
            CancellationToken cancellationToken = default)
            where TApp : class
        {
            return await RunCommandLineApplicationAsync<TApp>(hostBuilder, args, _ => { }, cancellationToken);
        }

        /// <summary>
        ///     Runs an instance of <typeparamref name="TApp" /> using <see cref="CommandLineApplication" /> to provide
        ///     command line parsing on the given <paramref name="args" />.  This method should be the primary approach
        ///     taken for command line applications.
        /// </summary>
        /// <typeparam name="TApp">The type of the command line application implementation</typeparam>
        /// <param name="hostBuilder">This instance</param>
        /// <param name="args">The command line arguments</param>
        /// <param name="configure">The delegate to configure the application</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task whose result is the exit code of the application</returns>
        public static async Task<int> RunCommandLineApplicationAsync<TApp>(
            this IHostBuilder hostBuilder,
            string[] args,
            Action<CommandLineApplication<TApp>> configure,
            CancellationToken cancellationToken = default)
            where TApp : class
        {
            using var host = hostBuilder.UseCommandLineApplication(args, configure).Build();
            return await host.RunCommandLineApplicationAsync(cancellationToken);
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
            configure ??= _ => { };
            var state = new CommandLineState(args);
            hostBuilder.Properties[typeof(CommandLineState)] = state;
            hostBuilder.ConfigureServices((context, services) =>
                services
                    .AddCommonServices(state)
                    .AddSingleton<ICommandLineService, CommandLineService>()
                    .AddSingleton(configure));

            using var host = hostBuilder.Build();
            return await host.RunCommandLineApplicationAsync(cancellationToken);
        }

        /// <summary>
        ///     Configures an instance of <typeparamref name="TApp" /> using <see cref="CommandLineApplication" /> to provide
        ///     command line parsing on the given <paramref name="args" />.
        /// </summary>
        /// <typeparam name="TApp">The type of the command line application implementation</typeparam>
        /// <param name="hostBuilder">This instance</param>
        /// <param name="args">The command line arguments</param>
        /// <returns><see cref="IHostBuilder"/></returns>
        public static IHostBuilder UseCommandLineApplication<TApp>(this IHostBuilder hostBuilder, string[] args)
            where TApp : class
            => UseCommandLineApplication<TApp>(hostBuilder, args, _ => { });


        /// <summary>
        ///     Configures an instance of <typeparamref name="TApp" /> using <see cref="CommandLineApplication" /> to provide
        ///     command line parsing on the given <paramref name="args" />.
        /// </summary>
        /// <typeparam name="TApp">The type of the command line application implementation</typeparam>
        /// <param name="hostBuilder">This instance</param>
        /// <param name="args">The command line arguments</param>
        /// <param name="configure">The delegate to configure the application</param>
        /// <returns><see cref="IHostBuilder"/></returns>
        public static IHostBuilder UseCommandLineApplication<TApp>(
            this IHostBuilder hostBuilder,
            string[] args,
            Action<CommandLineApplication<TApp>> configure)
            where TApp : class
        {
            configure ??= _ => { };
            var state = new CommandLineState(args);
            hostBuilder.Properties[typeof(CommandLineState)] = state;
            hostBuilder.ConfigureServices((context, services) =>
                services
                    .AddCommonServices(state)
                    .AddSingleton<ICommandLineService, CommandLineService<TApp>>()
                    .AddSingleton(configure));

            return hostBuilder;
        }

        private static IServiceCollection AddCommonServices(this IServiceCollection services, CommandLineState state)
        {
            services.TryAddSingleton<StoreExceptionHandler>();
            services.TryAddSingleton<IUnhandledExceptionHandler>(provider => provider.GetRequiredService<StoreExceptionHandler>());
            services.TryAddSingleton(PhysicalConsole.Singleton);
            services
                .AddSingleton<IHostLifetime, CommandLineLifetime>()
                .AddSingleton(provider =>
                {
                    state.SetConsole(provider.GetRequiredService<IConsole>());
                    return state;
                })
                .AddSingleton<CommandLineContext>(state);
            return services;
        }
    }
}
