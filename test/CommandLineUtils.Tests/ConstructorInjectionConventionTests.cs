// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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

        [SuppressDefaultHelpOption]
        private class OptionsCtorCommand
        {
            private readonly IEnumerable<CommandOption> _options;
            private readonly IEnumerable<CommandArgument> _arguments;

            public OptionsCtorCommand(IEnumerable<CommandOption> options, IEnumerable<CommandArgument> arguments)
            {
                _options = options;
                _arguments = arguments;
            }

            public int GetOptionCount() => _options.Count();
            public int GetArgCount() => _arguments.Count();

            [Option]
            public string Opt { get; }

            [Argument(0)]
            public string Arg { get; }
        }

        [Fact]
        public void ItPrefersIEnumOfOptionsFromUs()
        {
            var app = new CommandLineApplication<OptionsCtorCommand>();
            var services = new ServiceCollection().BuildServiceProvider();
            app.Conventions.UseDefaultConventions().UseConstructorInjection(services);
            app.Parse();
            Assert.Empty(services.GetServices<IEnumerable<CommandOption>>());
            Assert.Empty(services.GetServices<IEnumerable<CommandArgument>>());
            Assert.Equal(1, app.Model.GetOptionCount());
            Assert.Equal(1, app.Model.GetArgCount());
        }

        [Subcommand(typeof(Child))]
        private class Parent
        {

        }

        [Command("test")]
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
                .UseSubcommandAttributes()
                .UseCommandAttribute();

            var result = app.Parse("test");
            var subcmd = Assert.IsType<CommandLineApplication<Child>>(result.SelectedCommand);
            Assert.NotNull(subcmd.Model.Parent);
            Assert.Same(app.Model, subcmd.Model.Parent);
        }
    }
}
