using System.Threading;
using System.Threading.Tasks;

namespace McMaster.Extensions.Hosting.CommandLine
{
    /// <summary>
    /// A service to be run as part of the <see cref="CliLifetime"/>.
    /// </summary>
    /// <seealso cref="HostBuilderExtensions.RunCliAsync{T}(Microsoft.Extensions.Hosting.IHostBuilder, string[])"/>
    public interface ICliService
    {
        /// <summary>
        /// Runs the application asynchronously and returns the exit code.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
        /// <returns>The exit code</returns>
        Task<int> RunAsync(CancellationToken cancellationToken);
    }
}
