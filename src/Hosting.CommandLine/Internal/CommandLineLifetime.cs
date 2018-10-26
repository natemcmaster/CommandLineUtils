using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    /// <summary>
    /// Waits from completion of the <see cref="CommandLineApplication"/> and
    /// initiates shutdown.
    /// </summary>
    internal class CommandLineLifetime : IHostLifetime
    {
        private readonly ILogger<CommandLineLifetime> logger;
        private readonly IApplicationLifetime applicationLifetime;
        private readonly ICommandLineService cliService;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public CommandLineLifetime(ILogger<CommandLineLifetime> logger, IApplicationLifetime applicationLifetime,
            ICommandLineService cliService)
        {
            this.logger = logger;
            this.applicationLifetime = applicationLifetime;
            this.cliService = cliService;
        }

        /// <value>The exit code returned by the command line application</value>
        public int ExitCode { get; private set; } = 0;

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Registers an <code>ApplicationStarted</code> hook that runs the
        /// <see cref="ICommandLineService"/>. This ensures the container and all
        /// hosted services are started before the
        /// <see cref="CommandLineApplication"/> is run.  After the
        /// <code>ICliService</code> completes, the <code>ExitCode</code> is
        /// recorded and the application is stopped.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
        /// <returns></returns>
        /// <seealso cref="IHostLifetime.WaitForStartAsync(CancellationToken)"/>
        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            applicationLifetime.ApplicationStarted.Register(async () =>
            {
                ExitCode = await cliService.RunAsync(cancellationToken).ConfigureAwait(false);
                applicationLifetime.StopApplication();
            });
            return Task.CompletedTask;
        }
    }
}
