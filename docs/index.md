CommandLineUtils
================

**https://nuget.org/packages/McMaster.Extensions.CommandLineUtils**

This project is a fork of [Microsoft.Extensions.CommandLineUtils](https://github.com/aspnet/Common), which is no longer under [active development](https://github.com/aspnet/Common/issues/257). This fork, on the other hand, will continue release updates and take contributions.

## API Reference

See the [API Reference](./api/index.md) for more details.

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
  <PackageReference Include="McMaster.Extensions.CommandLineUtils" Version="2.2.5" />
</ItemGroup>
```

Pre-release builds and symbols: https://www.myget.org/gallery/natemcmaster/

## Usage

See [samples](https://github.com/natemcmaster/CommandLineUtils/tree/master/docs/samples) for more examples.

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
