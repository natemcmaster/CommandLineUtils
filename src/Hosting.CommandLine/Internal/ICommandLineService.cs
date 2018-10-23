using System.Threading;
using System.Threading.Tasks;

namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    /// <summary>
    /// A service to be run as part of the <see cref="CommandLineLifetime"/>.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.HostBuilderExtensions.RunCommandLineApplicationAsync{T}(Microsoft.Extensions.Hosting.IHostBuilder, string[])"/>
    internal interface ICommandLineService
    {
        /// <summary>
        /// Runs the application asynchronously and returns the exit code.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
        /// <returns>The exit code</returns>
        Task<int> RunAsync(CancellationToken cancellationToken);
    }
}
