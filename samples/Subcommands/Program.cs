// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;

namespace SubcommandSample
{
    /// <summary>
    /// This example is meant to show you how to structure a console application that uses
    /// the nested subcommands with options and arguments that vary between each subcommand.
    /// </summary>
    [Command(ThrowOnUnexpectedArgument = false)]
    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0), Range(1, 3)]
        public int? Option { get; set; }

        public string[] RemainingArgs { get; set; }

        private int OnExecute()
        {
            const string prompt = @"Which example would you like to run?
1 - Fake Git
2 - Fake Docker
3 - Fake npm
> ";
            if (!Option.HasValue)
            {
                Option = Prompt.GetInt(prompt);
            }

            if (RemainingArgs == null || RemainingArgs.Length == 0)
            {
                var args = Prompt.GetString("Enter some arguments >");
                RemainingArgs = args.Split(' ');
            }

            switch (Option)
            {
                case 1:
                    return CommandLineApplication.Execute<Git>(RemainingArgs);
                case 2:
                    return CommandLineApplication.Execute<Docker>(RemainingArgs);
                case 3:
                    return Npm.Main(RemainingArgs);
                default:
                    Console.Error.WriteLine("Unknown option");
                    return 1;
            }
        }
    }
}
