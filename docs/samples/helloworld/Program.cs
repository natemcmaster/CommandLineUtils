using System;
using McMaster.Extensions.CommandLineUtils;

var app = new CommandLineApplication();

app.HelpOption();
var optionSubject = app.Option("-s|--subject <SUBJECT>", "The subject", CommandOptionType.SingleValue);
var optionRepeat = app.Option<int>("-n|--count <N>", "Repeat", CommandOptionType.SingleValue);

app.OnExecute(() =>
{
    var subject = optionSubject.HasValue()
        ? optionSubject.Value()
        : "world";

    var count = optionRepeat.HasValue() ? optionRepeat.ParsedValue : 1;
    for (var i = 0; i < count; i++)
    {
        Console.WriteLine($"Hello {subject}!");
    }
    return 0;
});

return app.Execute(args);
