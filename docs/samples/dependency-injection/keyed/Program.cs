// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace KeyedServices
{
    [Command(Name = "keyed-di", Description = "Keyed Dependency Injection sample project")]
    [HelpOption]
    class Program
    {
        public static int Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddKeyedSingleton<IGreeter, FormalGreeter>("formal")
                .AddKeyedSingleton<IGreeter, CasualGreeter>("casual")
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>();
            app.Conventions.UseDefaultConventions()
                .UseConstructorInjection(services);
            return app.Execute(args);
        }

        [Option(Description = "The subject to greet")]
        public string Subject { get; set; } = "world";

        [Option(Description = "Use casual greeting")]
        public bool Casual { get; set; }

        private int OnExecute(
            [FromKeyedServices("formal")] IGreeter formalGreeter,
            [FromKeyedServices("casual")] IGreeter casualGreeter,
            IConsole console)
        {
            var greeter = Casual ? casualGreeter : formalGreeter;
            console.WriteLine(greeter.Greet(Subject));
            return 0;
        }
    }

    interface IGreeter
    {
        string Greet(string subject);
    }

    class FormalGreeter : IGreeter
    {
        public string Greet(string subject) => $"Good day, {subject}. How do you do?";
    }

    class CasualGreeter : IGreeter
    {
        public string Greet(string subject) => $"Hey {subject}!";
    }
}
