// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// This sample shows how to use dependency injection to inject services into your application
/// </summary>
class DependencyInjectionProgram
{
    public static int Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddSingleton<IMyService, MyServerImplementation>()
            .AddSingleton<IConsole>(PhysicalConsole.Singleton)
            .BuildServiceProvider();

        var app = new CommandLineApplication<DependencyInjectionProgram>();
        app.Conventions
            .UseDefaultConventions()
            .UseConstructorInjection(services);
        return app.Execute(args);
    }

    private readonly IMyService _myService;

    public DependencyInjectionProgram(IMyService myService)
    {
        _myService = myService;
    }

    private void OnExecute()
    {
        _myService.Invoke();
    }
}

interface IMyService
{
    void Invoke();
}

class MyServerImplementation : IMyService
{
    private readonly IConsole _console;

    public MyServerImplementation(IConsole console)
    {
        _console = console;
    }

    public void Invoke()
    {
        _console.WriteLine("Hello dependency injection!");
    }
}
