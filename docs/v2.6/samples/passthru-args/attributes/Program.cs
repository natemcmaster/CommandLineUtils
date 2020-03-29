using System;
using System.Diagnostics;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

[Command(
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    AllowArgumentSeparator = true)]
public class Program
{
    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    [Option("-m", Description = "Show time in milliseconds")]
    public bool Milliseconds { get; }

    public string[] RemainingArguments { get; } // = { "ls", "-a", "-l" }

    private void OnExecute()
    {
        var timer = Stopwatch.StartNew();
        if (RemainingArguments != null && RemainingArguments.Length > 0)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = RemainingArguments[0],
                    Arguments = ArgumentEscaper.EscapeAndConcatenate(RemainingArguments.Skip(1)),
                }
            };
            process.Start();
            process.WaitForExit();
        }

        timer.Stop();

        if (Milliseconds)
        {
            Console.WriteLine($"Time = {timer.Elapsed.TotalMilliseconds} ms");
        }
        else
        {
            Console.WriteLine($"Time = {timer.Elapsed.TotalSeconds}s");
        }
    }
}
