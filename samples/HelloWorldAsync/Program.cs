// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        app.HelpOption("-h|--help");
        var optionSubject = app.Option("-s|--subject <SUBJECT>", "The subject", CommandOptionType.SingleValue);

        app.OnExecute(async () =>
        {
            var subject = optionSubject.HasValue()
                ? optionSubject.Value()
                : "world";

            Console.Write($"Hello");

            // This pause here is just for indication that some awaitable operation could happens here.
            await Task.Delay(5000);
            Console.WriteLine($" {subject}!");
            return 0;
        });

        return app.Execute(args);
    }
}
