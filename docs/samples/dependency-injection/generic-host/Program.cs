using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.Hosting.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CustomServices
{
#region Program
    [Command(Name = "di", Description = "Dependency Injection sample project")]
    [HelpOption]
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await new HostBuilder()
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConsole();
                })
                .ConfigureServices((context, services) => {
                    services.AddSingleton<IMyService, MyServiceImplementation>()
                        .AddSingleton<IConsole>(PhysicalConsole.Singleton);
                })
                .RunCliAsync<Program>(args);
        }

        private readonly ILogger<Program> _logger;
        private readonly IMyService _myService;

        public Program(ILogger<Program> logger, IMyService myService)
        {
            _myService = myService;
            _logger = logger;
            
            _logger.LogInformation("Constructed!");
        }

        private void OnExecute()
        {
            _myService.Invoke();
        }
    }
#endregion

#region IMyService
    interface IMyService
    {
        void Invoke();
    }
#endregion

#region MyServiceImplementation
    class MyServiceImplementation : IMyService
    {
        private readonly IConsole _console;
        private readonly ILogger<MyServiceImplementation> _logger;

        public MyServiceImplementation(ILogger<MyServiceImplementation> logger,
            IConsole console)
        {
            _logger = logger;
            _console = console;

            _logger.LogInformation("Constructed!");
        }

        public void Invoke()
        {
            _console.WriteLine("Hello dependency injection!");
        }
    }
#endregion
}
