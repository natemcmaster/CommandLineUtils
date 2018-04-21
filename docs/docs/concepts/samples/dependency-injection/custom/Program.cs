using System;
using Microsoft.Extensions.DependencyInjection;
using McMaster.Extensions.CommandLineUtils;

namespace CustomServices
{
#region Program
    [Command(Name = "di", Description = "Dependency Injection sample project")]
    [HelpOption]
    class Program
    {
        public static int Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<IMyService, MyServiceImplementation>()
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);
            return app.Execute(args);
        }

        private readonly IMyService _myService;

        public Program(IMyService myService)
        {
            _myService = myService;
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

        public MyServiceImplementation(IConsole console)
        {
            _console = console;
        }

        public void Invoke()
        {
            _console.WriteLine("Hello dependency injection!");
        }
    }
#endregion
}
