using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace StandardServices
{
    [Command(Name = "di", Description = "Dependency Injection sample project")]
    [HelpOption]
    class Program
    {
        private readonly IConsole _console;
        
        static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

        public Program(IConsole console)
        {
            _console = console;
        }

        private int OnExecute()
        {
            _console.WriteLine("Hello from your first application");

            return 0;
        }
    }
}
