using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace SubcommandSample
{
    [Command("git")]
    [VersionOption("--version", "1.0.0")]
    [Subcommand("add", typeof(AddCommand))]
    [Subcommand("commit", typeof(CommitCommand))]
    class Program : CommandBase
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Option("--git-dir")]
        public string GitDir { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't displayed
            app.ShowHelp();
            return 1;
        }

        public override List<string> CreateArgs()
        {
            var args = new List<string>();

            if (GitDir != null)
            {
                args.Add("--git-dir=" + GitDir);
            }
            return args;
        }
    }

    [Command(Description = "Add file contents to the index")]
    class AddCommand : CommandBase
    {
        [Argument(0)]
        public string[] Files { get; set; }

         // this will automatically be set OnExecute
        private Program Parent { get; set; }

        public override List<string> CreateArgs()
        {
            var args = Parent.CreateArgs();
            args.Add("add");

            if (Files != null)
            {
                args.AddRange(Files);
            }

            return args;
        }
    }

    [Command(Description = "Record changes to the repository")]
    class CommitCommand : CommandBase
    {
        [Option("-m")]
        public string Message { get; set; }

        // this will automatically be set OnExecute
        private Program Parent { get; set; }

        public override List<string> CreateArgs()
        {
            var args = Parent.CreateArgs();
            args.Add("commit");

            if (Message != null)
            {
                args.Add("-m");
                args.Add(Message);
            }
            return args;
        }
    }

    [HelpOption("--help")]
    abstract class CommandBase
    {
        public abstract List<string> CreateArgs();

        protected virtual int OnExecute(CommandLineApplication app)
        {
            var args = CreateArgs();

            Console.WriteLine("=> git " + ArgumentEscaper.EscapeAndConcatenate(args));
            return 0;
        }
    }
}
