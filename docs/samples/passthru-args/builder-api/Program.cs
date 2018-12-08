using System;
using System.Diagnostics;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication(throwOnUnexpectedArg: false)
        {
            AllowArgumentSeparator = true
        };

        var showMilliseconds = app.Option<int>("-m", "Show time in milliseconds", CommandOptionType.NoValue);

        app.OnExecute(() =>
        {
            var timer = Stopwatch.StartNew();
            if (app.RemainingArguments != null && app.RemainingArguments.Count > 0)
            {
                var process = new Process
                {
                    StartInfo =
                {
                    FileName = app.RemainingArguments[0],
                    Arguments = ArgumentEscaper.EscapeAndConcatenate(app.RemainingArguments.Skip(1)),
                }
                };
                process.Start();
                process.WaitForExit();
            }

            timer.Stop();

            if (showMilliseconds.HasValue())
            {
                Console.WriteLine($"Time = {timer.Elapsed.TotalMilliseconds} ms");
            }
            else
            {
                Console.WriteLine($"Time = {timer.Elapsed.TotalSeconds}s");
            }
        });

        return app.Execute(args);
    }
}
