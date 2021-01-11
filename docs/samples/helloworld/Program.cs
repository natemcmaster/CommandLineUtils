using System;
using McMaster.Extensions.CommandLineUtils;

var app = new CommandLineApplication();

app.HelpOption();
var optionSubject = app.Option("-s|--subject <SUBJECT>", "The subject", CommandOptionType.SingleValue);
optionSubject.DefaultValue = "world";
var optionRepeat = app.Option<int?>("-n|--count <N>", "Repeat", CommandOptionType.SingleValue);
optionRepeat.DefaultValue = 1;

app.OnExecute(() =>
{
    for (var i = 0; i < optionRepeat.ParsedValue; i++)
    {
        Console.WriteLine($"Hello {optionSubject.Value()}!");
    }
    return 0;
});

return app.Execute(args);
