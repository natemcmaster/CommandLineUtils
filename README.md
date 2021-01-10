CommandLineUtils
================

[![Build Status](https://dev.azure.com/natemcmaster/github/_apis/build/status/CommandLineUtils?branchName=main)](https://dev.azure.com/natemcmaster/github/_build/?definitionId=3)

[![NuGet][nuget-badge] ![NuGet Downloads][nuget-download-badge]][nuget]

[nuget]: https://www.nuget.org/packages/McMaster.Extensions.CommandLineUtils/
[nuget-badge]: https://img.shields.io/nuget/v/McMaster.Extensions.CommandLineUtils.svg?style=flat-square
[nuget-download-badge]: https://img.shields.io/nuget/dt/McMaster.Extensions.CommandLineUtils?style=flat-square


This is a fork of [Microsoft.Extensions.CommandLineUtils](https://github.com/aspnet/Common), which is no longer under [active development](https://github.com/aspnet/Common/issues/257). This fork, on the other hand, will continue to make improvements, release updates, and accept contributions.

The roadmap for this project is [pinned to the top of the issue list](https://github.com/natemcmaster/CommandLineUtils/issues/).

## Install

Install the [NuGet package][nuget] into your project.

```
PM> Install-Package McMaster.Extensions.CommandLineUtils
```
```
$ dotnet add package McMaster.Extensions.CommandLineUtils
```

## Usage

See [documentation](https://natemcmaster.github.io/CommandLineUtils/) for API reference, samples, and tutorials.
See [docs/samples/](./docs/samples/) for more examples, such as:

 - [Async console apps](./docs/samples/helloworld-async/)
 - [Structuring an app with subcommands](./docs/samples/subcommands/)
 - [Defining options with attributes](./docs/samples/attributes/)
 - [Interactive console prompts](./docs/samples/interactive-prompts/)
 - [Required options and arguments](./docs/samples/validation/)

`CommandLineApplication` is the main entry point for most console apps parsing. There are two primary ways to use this API, using the builder pattern and attributes.

### Attribute API

```c#
using System;
using McMaster.Extensions.CommandLineUtils;

public class Program
{
    public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    [Option(Description = "The subject")]
    public string Subject { get; }

    [Option(ShortName = "n")]
    public int Count { get; }

    private void OnExecute()
    {
        var subject = Subject ?? "world";
        for (var i = 0; i < Count; i++)
        {
            Console.WriteLine($"Hello {subject}!");
        }
    }
}
```

### Builder API


```c#
using System;
using McMaster.Extensions.CommandLineUtils;

public class Program
{
    public static int Main(string[] args)
    {
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
    }
}

```

### Utilities

The library also includes other utilities for interaction with the console. These include:

- `ArgumentEscaper` - use to escape arguments when starting a new command line process.
    ```c#
     var args = new [] { "Arg1", "arg with space", "args ' with \" quotes" };
     Process.Start("echo", ArgumentEscaper.EscapeAndConcatenate(args));
    ```
 - `Prompt` - for getting feedback from users with a default answer.
   A few examples:
    ```c#
    // allows y/n responses, will return false by default in this case.
    // You may optionally change the prompt foreground and background color for
    // the message.
    Prompt.GetYesNo("Do you want to proceed?", false);

    // masks input as '*'
    Prompt.GetPassword("Password: ");
    ```
 - `DotNetExe` - finds the path to the dotnet.exe file used to start a .NET Core process
    ```c#
    Process.Start(DotNetExe.FullPathOrDefault(), "run");
    ```

And more! See the [documentation](https://natemcmaster.github.io/CommandLineUtils/) for more API, such as `IConsole`, `IReporter`, and others.
