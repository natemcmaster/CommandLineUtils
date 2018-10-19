namespace McMaster.Extensions.Hosting.CommandLine
{
    /// <summary>
    /// A DI container for storing command line arguments.
    /// </summary>
    /// <seealso cref="HostBuilderExtensions.UseCli{T}(Microsoft.Extensions.Hosting.IHostBuilder, string[])"/>
    public class CliArgs
    {
        /// <value>The command line arguments</value>
        public string[] Value {get; set;}
    }
}
