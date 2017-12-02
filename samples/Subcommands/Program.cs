// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils;

namespace SubcommandSample
{
    /// <summary>
    /// This example is meant to show you how to structure a console application that uses
    /// the nested subcommands with options and arguments that vary between each subcommand.
    /// </summary>
    class Program
    {
        public static int Main(string[] args)
        {
            const string prompt = @"Which example would you like to run?
1 - Fake Git
2 - Fake Docker
> ";
            var option = Prompt.GetInt(prompt);

            switch (option)
            {
                case 1:
                    return CommandLineApplication.Execute<Git>(args);
                case 2:
                    return CommandLineApplication.Execute<Docker>(args);
                default:
                    Console.Error.WriteLine("Unknown option");
                    return 1;
            }
        }
    }
}
