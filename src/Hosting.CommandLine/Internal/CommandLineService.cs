using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    /// <inheritdoc/>
    internal class CommandLineService<T> : IDisposable, ICommandLineService where T : class
    {
        private ILogger logger;
        private CommandLineApplication application;
        private CommandLineState state;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="logger">A logger</param>
        /// <param name="state">The command line state</param>
        /// <param name="serviceProvider">The DI service provider</param>
        public CommandLineService(ILogger<CommandLineService<T>> logger, CommandLineState state,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.state = state;

            logger.LogDebug("Constructing CommandLineApplication<{type}> with args [{args}]",
                typeof(T).FullName, String.Join(",", state.Arguments));
            application = new CommandLineApplication<T>();
            application.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(serviceProvider);
        }

        public void Dispose()
        {
            ((IDisposable)application).Dispose();
        }

        /// <inheritdoc/>
        public Task<int> RunAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("Running");
            return Task.Run(() => state.ExitCode = application.Execute(state.Arguments));
        }
    }
}
