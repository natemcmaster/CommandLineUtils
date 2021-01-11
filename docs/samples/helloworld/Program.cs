using System;
using McMaster.Extensions.CommandLineUtils;

var app = new CommandLineApplication();

app.HelpOption();
var subject = app.Option("-s|--subject <SUBJECT>", "The subject", CommandOptionType.SingleValue);
subject.DefaultValue = "world";
var repeat = app.Option<int?>("-n|--count <N>", "Repeat", CommandOptionType.SingleValue);
repeat.DefaultValue = 1;

app.OnExecute(() =>
{
    for (var i = 0; i < repeat.ParsedValue; i++)
    {
        Console.WriteLine($"Hello {subject.Value()}!");
    }
});

return app.Execute(args);
