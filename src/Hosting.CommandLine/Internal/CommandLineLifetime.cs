// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Hosting;

namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    /// <summary>
    ///     Waits from completion of the <see cref="CommandLineApplication" /> and
    ///     initiates shutdown.
    /// </summary>
    internal class CommandLineLifetime : IHostLifetime
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ICommandLineService _cliService;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public CommandLineLifetime(IApplicationLifetime applicationLifetime,
            ICommandLineService cliService)
        {
            _applicationLifetime = applicationLifetime;
            _cliService = cliService;
        }

        /// <summary>The exit code returned by the command line application</summary>
        public int ExitCode { get; private set; }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Registers an <code>ApplicationStarted</code> hook that runs the
        ///     <see cref="ICommandLineService" />. This ensures the container and all
        ///     hosted services are started before the
        ///     <see cref="CommandLineApplication" /> is run.  After the
        ///     <code>ICliService</code> completes, the <code>ExitCode</code> is
        ///     recorded and the application is stopped.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
        /// <returns></returns>
        /// <seealso cref="IHostLifetime.WaitForStartAsync(CancellationToken)" />
        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStarted.Register(async () =>
            {
                ExitCode = await _cliService.RunAsync(cancellationToken).ConfigureAwait(false);
                _applicationLifetime.StopApplication();
            });

            return Task.CompletedTask;
        }
    }
}
