---
uid: doc-intro
---
# Introduction

**CommandLineUtils** is a library which helps developers implement command line applications. The primary goal of the library is to assist with parsing command line arguments and executing the correct commands related to those arguments. However, the library also provides various other utilities such as input helpers.

## Installation

**CommandLineUtils** can be added to your project using NuGet.
Follow instructions on https://nuget.org/packages/McMaster.Extensions.CommandLineUtils that match your project type or editor.

The two common ways to do this are:

1. Using the [Package Manager Console](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio#package-manager-console):

    ```
    Install-Package McMaster.Extensions.CommandLineUtils
    ```

2. Using the [dotnet CLI](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-using-the-dotnet-cli):

    ```
    dotnet add package McMaster.Extensions.CommandLineUtils
    ```


## Your first console application

[CommandLineApplication](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplication) is the main entry point for most console apps parsing. There are two primary ways to use this API, using attributes or the builder pattern.

### Using Attributes

```csharp
using System;
using McMaster.Extensions.CommandLineUtils;

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

### Using the Builder Pattern

```csharp
using System;
using McMaster.Extensions.CommandLineUtils;

public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        app.HelpOption();
        var subject = app.Option("-s|--subject <SUBJECT>", "The subject", CommandOptionType.SingleValue);
        subject.DefaultValue = "world";

        app.OnExecute(() =>
        {
            Console.WriteLine($"Hello {subject.Value()}!");
            return 0;
        });

        return app.Execute(args);
    }
}
```

## Relationship to Microsoft.Extensions.CommandLineUtils

This project is a fork of [Microsoft.Extensions.CommandLineUtils](https://github.com/aspnet/Common), which is no longer under [active development](https://github.com/aspnet/Common/issues/257). This project, on the other hand, will continue release updates and take contributions.

## More information

For more information, you can refer to the sample applications.
