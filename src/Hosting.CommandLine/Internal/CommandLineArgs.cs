namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    /// <summary>
    /// A DI container for storing command line arguments.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.HostBuilderExtensions.UseCli{T}(Microsoft.Extensions.Hosting.IHostBuilder, string[])"/>
    internal class CommandLineArgs
    {
        /// <value>The command line arguments</value>
        public string[] Value {get; set;}
    }
}
