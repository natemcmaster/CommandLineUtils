using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace CommandLineParserSample
{
    [Command("HelloWorldSample", ExtendedHelpText = CommandLineOptions.HelpText)]
    class CommandLineOptions
    {
        [Argument(0)]
        public string Verb { get; set; } = "Hello";

        [Option(Description = "A list of subjects. Multiple values allowed.")]
        public IReadOnlyList<string> Subjects { get; set; }

        [HelpOption]
        public bool IsHelp { get; set; }

        [VersionOption("99.99.99")]
        public bool IsVersion { get; set; }

        private const string HelpText 
        = @"
Additional Info:
   This text is printed to the console when someone runs this app with `--help`.
";
    }

    class Program
    {
        static int Main(string[] args)
        {
            var options = CommandLineParser.ParseArgs<CommandLineOptions>(args);

            if (options.IsHelp)
            {
                return 2;
            }

            if (options.IsVersion)
            {
                return 3;
            }

            var subjects = options.Subjects ?? new [] { "world" };

            foreach (var subject in subjects)
            {
                Console.WriteLine($"Hello {subject}!");
            }

            return 0;
        }
    }
}
