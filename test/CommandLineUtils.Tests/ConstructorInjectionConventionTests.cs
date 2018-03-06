// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ConstructorInjectionConventionTests
    {
        private readonly ITestOutputHelper _output;

        public ConstructorInjectionConventionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class InjectedConsole
        {
            public IConsole Console { get; }

            public InjectedConsole(IConsole console)
            {
                Console = console;
            }
        }

        [Fact]
        public void ItInjectsModelTypeWithDefaultServices()
        {
            var app = new CommandLineApplication<InjectedConsole>();
            app.Conventions.UseConstructorInjection();
            app.Parse();
            Assert.NotNull(app.Model.Console);
            Assert.IsType<PhysicalConsole>(app.Model.Console);
        }

        [Fact]
        public void ItSupportsCustomServices()
        {
            var testConsole = new TestConsole(_output);
            var services = new ServiceCollection()
                .AddSingleton<IConsole>(testConsole)
                .BuildServiceProvider();
            var app = new CommandLineApplication<InjectedConsole>();
            app.Conventions.UseConstructorInjection(services);
            Assert.Same(testConsole, app.Model.Console);
        }

        [Subcommand("test", typeof(Child))]
        private class Parent
        {

        }

        private class Child
        {
            public Parent Parent { get; }

            public Child(Parent parent)
            {
                Parent = parent;
            }
        }

        [Fact]
        public void ItInjectsTheParentModel()
        {
            var app = new CommandLineApplication<Parent>();
            app.Conventions
                .UseConstructorInjection()
                .UseSubcommandAttributes();

            var result = app.Parse("test");
            var subcmd = Assert.IsType<CommandLineApplication<Child>>(result.SelectedCommand);
            Assert.NotNull(subcmd.Model.Parent);
            Assert.Same(app.Model, subcmd.Model.Parent);
        }
    }
}
