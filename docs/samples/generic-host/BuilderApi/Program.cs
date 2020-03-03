using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static Task<int> Main(string[] args)
        => new HostBuilder()
        .RunCommandLineApplicationFluentAsync(args, (app, provider) =>
        {
            var portOption = app.Option<int>("-p|--port <PORT>", "Port", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var port = portOption.HasValue() ? portOption.ParsedValue : 8080;
                var env = provider.GetRequiredService<IHostingEnvironment>();
                Console.WriteLine($"Starting on port {port}, env = {env.EnvironmentName}");
            });
        });
}
