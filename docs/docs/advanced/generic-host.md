---
uid: generic-host
---
# Integration with Generic Host

The McMaster.Extensions.Hosting.CommandLine package provides support for integrating command line parsing with
.NET's [generic host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host).

## Get started

To get started, install the `McMaster.Extensions.Hosting.CommandLine` package.
The main usage for generic host is `RunCommandLineApplicationAsync<TApp>(args)`, where `TApp` is a class
which will be bound to command line arguments and options using attributes and `CommandLineApplication.Execute<T>`.

### Sample

This minimal sample shows how to take advantage of generic host features, such as `IHostingEnvironment`,
as well as command line parsing options with this library.

[!code-csharp[](../../samples/generic-host/Program.cs)]

## Dependency injection

Generic host integration allows you to use the most current DI configuration approach indicated by the aspnet project.  The basic approach starts by creating the builder:

[!code-csharp[](../../samples/dependency-injection/generic-host/Program.cs?name=Program&range=26-26)]

Then you can configure your features:

[!code-csharp[](../../samples/dependency-injection/generic-host/Program.cs?name=Program&range=27-34)]

And finally, run your program:

[!code-csharp[](../../samples/dependency-injection/generic-host/Program.cs?name=Program&range=35-35)]

Below is the full source code for the generic host services example. Notice that instance of `IGreeter` will be injected into the `Program` constructor thanks to the dependency injection.

[!code-csharp[](../../samples/dependency-injection/custom/Program.cs?name=Program&highlight=32-32)]

