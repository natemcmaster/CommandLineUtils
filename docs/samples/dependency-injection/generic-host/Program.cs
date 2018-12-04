using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.Hosting.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomServices
{
#region Program
    [Command(Name = "di", Description = "Dependency Injection sample project")]
    class Program
    {

        [Argument(0, Description = "your name")]
        private string Name { get; } = "dependency injection";

        [Option("-l|--language", Description = "your desired language")]
        [AllowedValues("english", "spanish", IgnoreCase = true)]
        public string Language { get; } = "english";

        public static async Task<int> Main(string[] args)
        {
            return await new HostBuilder()
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConsole();
                })
                .ConfigureServices((context, services) => {
                    services.AddSingleton<IGreeter, Greeter>()
                        .AddSingleton<IConsole>(PhysicalConsole.Singleton);
                })
                .RunCommandLineApplicationAsync<Program>(args);
        }

        private readonly ILogger<Program> _logger;
        private readonly IGreeter _greeter;

        public Program(ILogger<Program> logger, IGreeter greeter)
        {
            _logger = logger;
            _greeter = greeter;

            _logger.LogInformation("Constructed!");
        }

        private void OnExecute()
        {
            _greeter.Greet(Name, Language);
        }
    }
#endregion

#region IGreeter
    interface IGreeter
    {
        void Greet(string name, string language);
    }
#endregion

#region Greeter
    class Greeter : IGreeter
    {
        private readonly IConsole _console;
        private readonly ILogger<Greeter> _logger;

        public Greeter(ILogger<Greeter> logger,
            IConsole console)
        {
            _logger = logger;
            _console = console;

            _logger.LogInformation("Constructed!");
        }

        public void Greet(string name, string language = "english")
        {
            string greeting;
            switch (language)
            {
                case "english": greeting = "Hello {0}"; break;
                case "spanish": greeting = "Hola {0}"; break;
                default: throw new InvalidOperationException("validation should have caught this");
            }
            _console.WriteLine(greeting, name);
        }
    }
#endregion
}
