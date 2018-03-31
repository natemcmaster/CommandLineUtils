CommandLineUtils
================

[![Travis build status][travis-badge]](https://travis-ci.org/natemcmaster/CommandLineUtils/branches)
[![AppVeyor build status][appveyor-badge]](https://ci.appveyor.com/project/natemcmaster/CommandLineUtils/branch/master)

[travis-badge]: https://img.shields.io/travis/natemcmaster/CommandLineUtils/master.svg?label=travis&style=flat-square
[appveyor-badge]: https://img.shields.io/appveyor/ci/natemcmaster/CommandLineUtils/master.svg?label=appveyor&style=flat-square

[![NuGet][main-nuget-badge]][main-nuget] [![MyGet][main-myget-badge]][main-myget]

[main-nuget]: https://www.nuget.org/packages/McMaster.Extensions.CommandLineUtils/
[main-nuget-badge]: https://img.shields.io/nuget/v/McMaster.Extensions.CommandLineUtils.svg?style=flat-square&label=nuget
[main-myget]: https://www.myget.org/feed/natemcmaster/package/nuget/McMaster.Extensions.CommandLineUtils
[main-myget-badge]: https://img.shields.io/www.myget/natemcmaster/vpre/McMaster.Extensions.CommandLineUtils.svg?style=flat-square&label=myget


This is a fork of [Microsoft.Extensions.CommandLineUtils](https://github.com/aspnet/Common), which is no longer under [active development](https://github.com/aspnet/Common/issues/257). This fork, on the other hand, will continue release updates and take contributions.

## Install

Install the NuGet package into your project.

```
PM> Install-Package McMaster.Extensions.CommandLineUtils
```
```
$ dotnet add package McMaster.Extensions.CommandLineUtils
```
```xml
<ItemGroup>
  <PackageReference Include="McMaster.Extensions.CommandLineUtils" Version="2.2.0" />
</ItemGroup>
```

Pre-release builds and symbols: https://www.myget.org/gallery/natemcmaster/

## Usage

See [samples/](./samples/) for more examples, such as:

 - [Async console apps](./samples/HelloWorld/Program.cs)
 - [Structing an app with subcommands](./samples/Subcommands/Program.cs)
 - [Defining options with attributes](./samples/Attributes/Program.cs)
 - [Interactive console prompts](./samples/Prompt/Program.cs)
 - [Required options and arguments](./samples/Validation/Program.cs)

`CommandLineApplication` is the main entry point for most console apps parsing. There are two primary ways to use this API, using the builder pattern and attributes.

### Attribute API

```c#
using System;
using McMaster.Extensions.CommandLineUtils;

[HelpOption]
public class Program
{
    public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    [Option(Description = "The subject")]
    public string Subject { get; }

    private void OnExecute()
    {
        var subject = Subject ?? "world";
        Console.WriteLine($"Hello {subject}!");
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

        app.OnExecute(() =>
        {
            var subject = optionSubject.HasValue()
                ? optionSubject.Value()
                : "world";

            Console.WriteLine($"Hello {subject}!");
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
     var args = new [] { "Arg1", "arg with space", "args ' with \"" quotes" };
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

And more! See the docs for more API, such as `IConsole`, `IReporter`, and others.
