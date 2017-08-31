CommandLineUtils
================

[![AppVeyor build status][appveyor-badge]](https://ci.appveyor.com/project/natemcmaster/CommandLineUtils/branch/dev)

[appveyor-badge]: https://img.shields.io/appveyor/ci/natemcmaster/CommandLineUtils/dev.svg?label=appveyor&style=flat-square

This is a fork of [Microsoft.Extensions.CommandLineUtils](https://github.com/aspnet/Common). This project originally started as an ASP.NET Core class library. However, after a few releases, [active development was discontinued](https://github.com/aspnet/Common/issues/257).

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
  <PackageReference Include="McMaster.Extensions.CommandLineUtils" Version="2.0.0" />
</ItemGroup>
```

## Usage

See [samples/](./samples/) for more examples.

`CommandLineApplication` is the main entry point for most console apps parsing.

```c#
using System;
using McMaster.Extensions.CommandLineUtils;

public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        app.HelpOption("-h|--help");
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
