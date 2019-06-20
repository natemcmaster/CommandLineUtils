// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.ExceptionServices;
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
    internal class CommandLineLifetime : IHostLifetime, IDisposable
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ICommandLineService _cliService;
        private readonly IConsole _console;
        private readonly IUnhandledExceptionHandler? _unhandledExceptionHandler;
        private readonly ManualResetEvent _disposeComplete = new ManualResetEvent(false);

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public CommandLineLifetime(IApplicationLifetime applicationLifetime,
            ICommandLineService cliService,
            IConsole console,
            IUnhandledExceptionHandler? unhandledExceptionHandler = null)
        {
            _applicationLifetime = applicationLifetime;
            _cliService = cliService;
            _console = console;
            _unhandledExceptionHandler = unhandledExceptionHandler;
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
                try
                {
                    ExitCode = await _cliService.RunAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (_unhandledExceptionHandler != null)
                    {
                        _unhandledExceptionHandler.HandleException(e);
                    }
                    else
                    {
                        ExceptionDispatchInfo.Capture(e).Throw();
                    }
                }
                finally
                {
                    _applicationLifetime.StopApplication();
                }
            });

            // Ensures services are disposed before the application exits.
            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                _applicationLifetime.StopApplication();
                _disposeComplete.WaitOne();
            };

            // Capture CTRL+C and prevent it from immediately force killing the app.
            _console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                _applicationLifetime.StopApplication();
            };

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _disposeComplete.Set();
            _disposeComplete.Dispose();
        }
    }
}
