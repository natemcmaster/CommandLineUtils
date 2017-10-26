using System;
using McMaster.Extensions.CommandLineUtils;

namespace HelloWorld
{
    class CommandLineOptions
    {
        [Option]
        public string Subject { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = CommandLineApplication.ParseArgs<CommandLineOptions>(args);
            var subject = options.Subject ?? "world";
            Console.WriteLine($"Hello {subject}!");
        }
    }
}
