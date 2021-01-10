using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

var app = new CommandLineApplication();

app.HelpOption();
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

        // Pause for dramatic effect
        await Task.Delay(2000, cancellationToken);

        Console.WriteLine($" {subject}!");
    }
});

return app.Execute(args);
