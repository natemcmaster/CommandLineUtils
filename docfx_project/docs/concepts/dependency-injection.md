# Dependency Injection

CommandLineUtils allow you to use dependency injection to inject dependencies into command constructors at runtime. This give you the ability to avoid tight coupling bewteen the commands and and the services which they depend on. CommandLineUtils has a standard set of services which are available to inject and also allow you to register and inject your own services.

## Using dependency injection

In order to inject services into a command constructor you need to specify the services to be injected as parameters for the **public constructor** of the command. 

In the example below, the `IConsole` implementation is injected into the constructor and stored in a field named `_console`. Later in the program, this is used to write output to the console:

[!code-csharp[](./samples/dependency-injection/standard/Program.cs?range=7-26&highlight=5,9-12,16)]

## Using the standard services

CommandLineApplication makes a number of services available by default for injecting into your command constructors. There are the standard services which you can inject:

Service | Description
---------|----------
`CommandLineApplication` | Injects the [CommandLineApplication](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplication) instance.
`IConsole` | Injects the [IConsole](xref:McMaster.Extensions.CommandLineUtils.IConsole) implementation.
`IEnumerable<CommandOption>` | Injects the definitions for the [options](xref:McMaster.Extensions.CommandLineUtils.CommandOption) passed to the command.
`IEnumerable<CommandArgument>` | Injects the definitions for the [arguments](xref:McMaster.Extensions.CommandLineUtils.CommandArgument) passed to the command.
`CommandLineContext` | Injects the [CommandLineContext](xref:McMaster.Extensions.CommandLineUtils.Abstractions.CommandLineContext) which contains information abount the execution context of the command.
`IServiceProvider` | ...
Command parent type | When using sub-commands, you can inject the type of the parent command into the constructor for a sub-command.

## Registering your own services

You can register your own services by using the [ConstructorInjectionConvention](McMaster.Extensions.CommandLineUtils.Conventions.ConstructorInjectionConvention) and passing an `IServiceProvider` instance 

In the example below, the standard .NET Core `ServiceCollection` class is used to register the impementation of `IMyService`. `ServiceColl...`

