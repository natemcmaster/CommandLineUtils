// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

/// <summary>
/// You can call <see cref="CommandLineApplication.OnExecute(Func{Task{int}})" />
/// with an async lambda to have your application execute asynchronously.
/// </summary>
public class AsyncWithBuilderApi
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        app.HelpOption("-h|--help");
        var optionSubject = app.Option("-s|--subject <SUBJECT>", "The subject", CommandOptionType.SingleValue);
        var optionRepeat = app.Option<int>("-n|--count <N>", "Repeat", CommandOptionType.SingleValue);

        app.OnExecuteAsync(async cancellationToken =>
        {
            var subject = optionSubject.HasValue()
                ? optionSubject.Value()
                : "world";

            var count = optionRepeat.HasValue() ? optionRepeat.ParsedValue : 1;
            for (var i = 0; i < count; i++)
            {
                Console.Write($"Hello");

                // This pause here is just for indication that some awaitable operation could happens here.
                await Task.Delay(5000, cancellationToken);
                Console.WriteLine($" {subject}!");
            }
        });

        return app.Execute(args);
    }
}
