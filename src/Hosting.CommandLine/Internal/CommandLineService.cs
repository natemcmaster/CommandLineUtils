// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    /// <summary>
    /// A service to be run as part of the <see cref="CommandLineLifetime"/> when using builder API.
    /// </summary>
    internal class CommandLineService : IDisposable, ICommandLineService
    {
        private readonly CommandLineApplication _application;
        private readonly ILogger _logger;
        private readonly CommandLineState _state;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="logger">A logger</param>
        /// <param name="state">The command line state</param>
        /// <param name="serviceProvider">The DI service provider</param>
        /// <param name="configure">The delegate to configure the app</param>
        public CommandLineService(ILogger<CommandLineService> logger, CommandLineState state,
            IServiceProvider serviceProvider, Action<CommandLineApplication> configure)
        {
            _logger = logger;
            _state = state;

            logger.LogDebug("Constructing CommandLineApplication with args [{args}]", string.Join(",", state.Arguments));
            _application = new CommandLineApplication(state.Console, state.WorkingDirectory);

            _application.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(serviceProvider);
            foreach (var convention in serviceProvider.GetServices<IConvention>())
            {
                _application.Conventions.AddConvention(convention);

            }

            configure(_application);
        }

        /// <inheritdoc />
        public async Task<int> RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Running");
            _state.ExitCode = await _application.ExecuteAsync(_state.Arguments, cancellationToken);
            return _state.ExitCode;
        }

        public void Dispose()
        {
            _application.Dispose();
        }
    }
}
