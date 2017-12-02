// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace SubcommandSample
{
    /// <summary>
    /// In this example, each sub command type inherits from <see cref="GitCommandBase"/>,
    /// which provides shared functionality between all the commands.
    /// This example also shows you how the subcommands can be linked to their parent types.
    /// </summary>
    [Command("fake-git")]
    [VersionOption("--version", "1.0.0")]
    [Subcommand("add", typeof(AddCommand))]
    [Subcommand("commit", typeof(CommitCommand))]
    class Git : GitCommandBase
    {
        [Option("--git-dir")]
        public string GitDir { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
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
    class AddCommand : GitCommandBase
    {
        [Argument(0)]
        public string[] Files { get; set; }

        // You can use this pattern when the parent command may have options or methods you want to
        // use from sub-commands.
        // This will automatically be set before OnExecute is invoked
        private Git Parent { get; set; }

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
    class CommitCommand : GitCommandBase
    {
        [Option("-m")]
        public string Message { get; set; }

        // This will automatically be set before OnExecute is invoked.
        private Git Parent { get; set; }

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

    /// <summary>
    /// This base type provides shared functionality.
    /// Also, declaring <see cref="HelpOptionAttribute"/> on this type means all types that inherit from it
    /// will automatically support '--help'
    /// </summary>
    [HelpOption("--help")]
    abstract class GitCommandBase
    {
        public abstract List<string> CreateArgs();

        protected virtual int OnExecute(CommandLineApplication app)
        {
            var args = CreateArgs();

            Console.WriteLine("Result = git " + ArgumentEscaper.EscapeAndConcatenate(args));
            return 0;
        }
    }
}
