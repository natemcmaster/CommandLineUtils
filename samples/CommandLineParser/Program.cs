// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils;

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

        var subjects = options.Subjects ?? new[] { "world" };

        foreach (var subject in subjects)
        {
            Console.WriteLine($"Hello {subject}!");
        }

        return 0;
    }
}
