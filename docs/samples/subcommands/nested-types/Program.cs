// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;

namespace SubcommandSample
{
    /// <summary>
    /// In this example, each command a nested class type.
    /// </summary>
    [Command(Name = "fake-docker", Description = "A self-sufficient runtime for containers"),
     Subcommand(typeof(Containers), typeof(Images))]
    class Docker
    {
        public static void Main(string[] args) => CommandLineApplication.Execute<Docker>(args);

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("You must specify at a subcommand.");
            app.ShowHelp();
            return 1;
        }

        /// <summary>
        /// <see cref="HelpOptionAttribute"/> must be declared on each type that supports '--help'.
        /// Compare to the inheritance example, in which <see cref="GitCommandBase"/> delcares it
        /// once so that all subcommand types automatically support '--help'.
        /// </summary>
        [Command("containers", Description = "Manage containers"),
         Subcommand(typeof(List)),
         Subcommand(typeof(Run))]
        private class Containers
        {
            private int OnExecute(IConsole console)
            {
                console.Error.WriteLine("You must specify an action. See --help for more details.");
                return 1;
            }

            [Command(Description = "List containers"), HelpOption]
            private class List
            {
                [Option(Description = "Show all containers (default shows just running)")]
                public bool All { get; }

                private void OnExecute(IConsole console)
                {
                    console.WriteLine(string.Join("\n",
                        "CONTAINERS",
                        "----------------",
                        "jubilant_jackson",
                        "lucid_torvalds"));
                }
            }

            [Command("run", Description = "Run a command in a new container",
                AllowArgumentSeparator = true,
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
            private class Run
            {
                [Required(ErrorMessage = "You must specify the image name")]
                [Argument(0, Description = "The image for the new container")]
                public string Image { get; }

                [Option("--name", Description = "Assign a name to the container")]
                public string Name { get; }

                /// <summary>
                /// When ThrowOnUnexpectedArgument is valids, any unrecognized arguments
                /// will be collected and set in this property, when set.
                /// </summary>
                public string[] RemainingArguments { get; }

                private void OnExecute(IConsole console)
                {
                    console.WriteLine(
                        $"Would have run {Image} (name = {Name}) with args => {ArgumentEscaper.EscapeAndConcatenate(RemainingArguments)}");
                }
            }
        }

        [Command("image",
             Description = "Manage images"),
         Subcommand(typeof(List))]
        private class Images
        {
            private int OnExecute(IConsole console)
            {
                console.Error.WriteLine("You must specify an action. See --help for more details.");
                return 1;
            }


            [Command("ls", Description = "List images",
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
            private class List
            {
                [Option(Description = "Show all containers (default shows just running)")]
                public bool All { get; }

                private IReadOnlyList<string> RemainingArguments { get; }

                private void OnExecute(IConsole console)
                {
                    console.WriteLine(string.Join("\n",
                        "IMAGES",
                        "--------------------",
                        "microsoft/dotnet:2.0"));
                }
            }
        }
    }
}
