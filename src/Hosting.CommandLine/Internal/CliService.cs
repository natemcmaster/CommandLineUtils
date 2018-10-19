using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    /// <inheritdoc/>
    internal class CliService<T> : ICliService where T : class
    {
        private ILogger logger;
        private CommandLineApplication application;
        private CliArgs args;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="logger">A logger</param>
        /// <param name="args">The command line arguments</param>
        /// <param name="serviceProvider">The DI service provider</param>
        public CliService(ILogger<CliService<T>> logger, CliArgs args,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.args = args;

            logger.LogDebug("Constructing CommandLineApplication<{type}> with args [{args}]",
                typeof(T).FullName, String.Join(",", args.Value));
            application = new CommandLineApplication<T>();
            application.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(serviceProvider);
        }

        /// <inheritdoc/>
        public Task<int> RunAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("Running");
            return Task.Run(() => application.Execute(args.Value));
        }
    }
}
