# Introduction

**CommandLineUtils** is a library which helps developers implement command line applications. The primary goal of the library is to assist with parsing command line arguments and executing the correct commands related to those arguments. However, the library also provides various other utilities such as input helpers.

## Installation

**CommandLineUtils** can be added to your project using NuGet in any one of the following ways:

1. Using the [Package Manager Console](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio#package-manager-console):

    ```
    PM> Install-Package McMaster.Extensions.CommandLineUtils
    ```

2. Using the [dotnet CLI](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-using-the-dotnet-cli):

    ```
    $ dotnet add package McMaster.Extensions.CommandLineUtils
    ```

3. Editing your `.csproj` file directly:

    ```xml
    <ItemGroup>
    <PackageReference Include="McMaster.Extensions.CommandLineUtils" Version="2.2.3" />
    </ItemGroup>
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

## More information

For more information, you can refer to the sample applications.
