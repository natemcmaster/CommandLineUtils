CommandLineUtils
================

[![Build Status](https://dev.azure.com/natemcmaster/github/_apis/build/status/CommandLineUtils?branchName=master)](https://dev.azure.com/natemcmaster/github/_build/latest?definitionId=3&branchName=master)

[![NuGet][main-nuget-badge]][main-nuget] [![MyGet][main-myget-badge]][main-myget]

[main-nuget]: https://www.nuget.org/packages/McMaster.Extensions.CommandLineUtils/
[main-nuget-badge]: https://img.shields.io/nuget/v/McMaster.Extensions.CommandLineUtils.svg?style=flat-square&label=nuget
[main-myget]: https://www.myget.org/feed/natemcmaster/package/nuget/McMaster.Extensions.CommandLineUtils
[main-myget-badge]: https://img.shields.io/www.myget/natemcmaster/vpre/McMaster.Extensions.CommandLineUtils.svg?style=flat-square&label=myget


This is a fork of [Microsoft.Extensions.CommandLineUtils](https://github.com/aspnet/Common), which is no longer under [active development](https://github.com/aspnet/Common/issues/257). This fork, on the other hand, will continue to make improvements, release updates and take contributions.

## Install

Install the [NuGet package][main-nuget] into your project.

```
PM> Install-Package McMaster.Extensions.CommandLineUtils
```
```
$ dotnet add package McMaster.Extensions.CommandLineUtils
```

Pre-release builds and symbols: https://www.myget.org/gallery/natemcmaster/

## Usage

See [documentation](https://natemcmaster.github.io/CommandLineUtils/) for API reference, samples, and tutorials.
See [samples/](./docs/samples/) for more examples, such as:

 - [Async console apps](./docs/samples/helloworld-async/)
 - [Structing an app with subcommands](./docs/samples/subcommands/)
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
 - `Prompt` - for getting feedback from users. A few examples:
    ```c#
    // allows y/n responses
    Prompt.GetYesNo("Do you want to proceed?");

    // masks input as '*'
    Prompt.GetPassword("Password: ");
    ```
 - `DotNetExe` - finds the path to the dotnet.exe file used to start a .NET Core process
    ```c#
    Process.Start(DotNetExe.FullPathOrDefault(), "run");
    ```

And more! See the [documentation](https://natemcmaster.github.io/CommandLineUtils/) for more API, such as `IConsole`, `IReporter`, and others.
