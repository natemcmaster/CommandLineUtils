using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

var app = new CommandLineApplication();

app.HelpOption();
var subject = app.Option("-s|--subject <SUBJECT>", "The subject", CommandOptionType.SingleValue);
subject.DefaultValue = "world";

var repeat = app.Option<int>("-n|--count <N>", "Repeat", CommandOptionType.SingleValue);
repeat.DefaultValue = 1;

app.OnExecuteAsync(async cancellationToken =>
{
    for (var i = 0; i < repeat.ParsedValue; i++)
    {
        Console.Write($"Hello");

        // Pause for dramatic effect
        await Task.Delay(2000, cancellationToken);

        Console.WriteLine($" {subject.Value()}!");
    }
});

return app.Execute(args);
