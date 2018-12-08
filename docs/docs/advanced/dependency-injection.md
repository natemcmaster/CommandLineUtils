---
uid: dependency-injection
---
# Dependency Injection

CommandLineUtils allow you to use dependency injection to inject dependencies into command constructors at runtime. This gives you the ability to avoid tight coupling between the commands and the services which they depend on. CommandLineUtils has a standard set of services which are available to inject and also allow you to register and inject your own services.

## Using dependency injection

To inject services into a command constructor, you need to specify the services to be injected as parameters for the **public constructor** of the command.

In the example below, the `IConsole` implementation is injected into the constructor and stored in a field named `_console`. Later in the program, this is used to write output to the console:

[!code-csharp[](../../samples/dependency-injection/standard/Program.cs?range=7-26&highlight=5,9-12,16)]

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

## Using your own services

You can register your own services by using the [ConstructorInjectionConvention](xref:McMaster.Extensions.CommandLineUtils.Conventions.ConstructorInjectionConvention) and making use of the `Microsoft.Extensions.DependencyInjection` NuGet package to contruct services.

In the example below, we have defined a service named `IMyService` with a single method named `Invoke`:

[!code-csharp[](../../samples/dependency-injection/custom/Program.cs?name=IMyService)]

The implementation of this service is done in `MyServiceImplementation`, which will write a string to the console. Also, note that an instance of `IConsole`  is injected into the `MyServiceImplementation` constructor.

[!code-csharp[](../../samples/dependency-injection/custom/Program.cs?name=MyServiceImplementation)]

You can register your own services by creating an instance of `ServiceCollection` and adding the services to the collection. A service provider is then created by calling the `BuildServiceProvider` method:

[!code-csharp[](../../samples/dependency-injection/custom/Program.cs?range=14-17)]

> [!NOTE]
> Take note that standard services which need to be injected into your custom services, such as `IConsole` which needs to be injected into the `MyServiceImplementation` constructor, needs to be added to the service collection as well.

Next, you can call add the constructor injection convention by calling `UseConstructorInjection`, passing the service provider which was previously created.

[!code-csharp[](../../samples/dependency-injection/custom/Program.cs?range=19-22)]

Below is the full source code for the custom services example. Notice that instance of `IMyService` will be injected into the `Program` constructor thanks to the dependency injection.

[!code-csharp[](../../samples/dependency-injection/custom/Program.cs?name=Program&highlight=21-24)]

## Using Generic Host

See <xref:generic-host> for details on using Generic Host and dependency injection.
