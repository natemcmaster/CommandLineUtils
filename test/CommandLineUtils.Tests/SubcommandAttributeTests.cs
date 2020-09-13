// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using McMaster.Extensions.CommandLineUtils.Errors;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class SubcommandAttributeTests
    {
        private readonly ITestOutputHelper _output;

        public SubcommandAttributeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Subcommand(
            typeof(AddCmd),
            typeof(RemoveCmd))]
        private class Program
        {
            public object? Subcommand { get; set; }
        }

        [Command("add", "a", "addition")]
        private class AddCmd
        {
            public object? Parent { get; }
        }

        [Command("rm")]
        private class RemoveCmd
        { }

        [Fact]
        public void AddsSubcommands()
        {
            var app = new CommandLineApplication<Program>();
            app.Conventions.UseSubcommandAttributes().UseCommandAttribute();
            Assert.Collection(app.Commands.OrderBy(c => c.Name),
                add => { Assert.Equal(new[] { "add", "a", "addition" }, add.Names.ToArray()); },
                rm => { Assert.Equal("rm", rm.Name); });
        }

        [Command(Name = "master")]
        [Subcommand(typeof(Level1Command))]
        private class MasterApp : CommandBase
        {
            [Option(Inherited = true)]
            public bool Verbose { get; set; }

            protected override int OnExecute(CommandLineApplication app) => 100;
        }

        [Subcommand(typeof(Level2Command))]
        private class Level1Command : CommandBase
        {
            [Option("--mid")]
            public bool Mid { get; }

            protected override int OnExecute(CommandLineApplication app) => 101;

            public MasterApp? Parent { get; }
        }

        private class Level2Command : CommandBase
        {
            [Option("--value")]
            public int? Value { get; set; }

            protected override int OnExecute(CommandLineApplication app)
                => Value.HasValue ? Value.Value : 102;

            public Level1Command? Parent { get; }
        }

        abstract class CommandBase
        {
            [HelpOption("--help")]
            protected bool IsHelp { get; }

            public CommandBase? Subcommand { get; set; }

            protected abstract int OnExecute(CommandLineApplication app);
        }

        [Fact]
        public void ItInvokesExecuteOnSubcommand_Bottom()
        {
            var rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output), "level1", "level2", "--value",
                "6");
            Assert.Equal(6, rc);

            rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output), "level1", "level2");
            Assert.Equal(102, rc);
        }

        [Fact]
        public void ItInvokesExecuteOnSubcommand_Middle()
        {
            var rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output), "level1");
            Assert.Equal(101, rc);
        }

        [Fact]
        public void ItInvokesExecuteOnSubcommand_Top()
        {
            var rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output));
            Assert.Equal(100, rc);
        }

        [Fact]
        public void HandlesHelp_Bottom()
        {
            var sb = new StringBuilder();
            var output = new StringWriter(sb);
            var console = new TestConsole(_output)
            {
                Out = output,
            };
            var rc = CommandLineApplication.Execute<MasterApp>(console, "level1", "level2", "--help");
            Assert.Equal(0, rc);
            Assert.Contains("Usage: master level1 level2 [options]", sb.ToString());
        }

        [Fact]
        public void HandlesHelp_Middle()
        {
            var sb = new StringBuilder();
            var output = new StringWriter(sb);
            var console = new TestConsole(_output)
            {
                Out = output,
            };
            var rc = CommandLineApplication.Execute<MasterApp>(console, "level1", "--help");
            Assert.Equal(0, rc);
            Assert.Contains("Usage: master level1 [command] [options]", sb.ToString());
        }

        [Fact]
        public void HandlesHelp_Top()
        {
            var sb = new StringBuilder();
            var output = new StringWriter(sb);
            var console = new TestConsole(_output)
            {
                Out = output,
            };
            var rc = CommandLineApplication.Execute<MasterApp>(console, "--help");
            Assert.Equal(0, rc);
            Assert.Contains("Usage: master [command] [options]", sb.ToString());
        }

        [Fact]
        public void ItCreatesNestedSubCommands()
        {
            var app = new CommandLineApplication<MasterApp>();
            app.Conventions.UseSubcommandAttributes().UseCommandAttribute().UseCommandNameFromModelType();
            var lvl1 = Assert.Single(app.Commands);
            Assert.Equal("level1", lvl1.Name);
            var lvl2 = Assert.Single(lvl1.Commands);
            Assert.Equal("level2", lvl2.Name);
        }

        [Fact]
        public void ItBindsOptionsOnParentItems()
        {
            var app = CommandLineParser.ParseArgs<MasterApp>("level1", "--mid", "level2", "--verbose", "--value", "6");
            Assert.IsType<Level1Command>(app.Subcommand);
            var sub = Assert.IsType<Level2Command>(app.Subcommand?.Subcommand);
            Assert.NotNull(sub.Parent);
            Assert.NotNull(sub.Parent?.Parent);
            Assert.True(sub.Parent?.Mid);
            Assert.True(sub.Parent?.Parent?.Verbose);
            Assert.Equal(6, sub.Value);
        }

        [Subcommand(typeof(Level1Command), typeof(Level1DuplicateCmd))]
        private class DuplicateSubCommands
        {
            private void OnExecute()
            { }
        }

        [Command("LEVEL1")]
        private class Level1DuplicateCmd
        { }

        [Fact]
        public void CommandNamesCannotDifferByCaseOnly()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CommandLineApplication.Execute<DuplicateSubCommands>(new TestConsole(_output)));
            Assert.Equal(Strings.DuplicateSubcommandName("level1"), ex.Message);
        }

        [Command, Subcommand(typeof(CycledCommand2))]
        private class CycledCommand1
        {
        }

        [Command, Subcommand(typeof(CycledCommand1))]
        private class CycledCommand2
        {
        }

        [Fact]
        public void ThrowsForCycledSubCommand()
        {
            var ex = Assert.Throws<SubcommandCycleException>(
                () => CommandLineApplication.Execute<CycledCommand1>(new TestConsole(_output)));
            Assert.Equal(typeof(CycledCommand1), ex.ModelType);
        }

        [Command, Subcommand(typeof(SelfCycledCommand))]
        private class SelfCycledCommand
        {

        }

        [Fact]
        public void ThrowsForSelfCycledCommand()
        {
            var ex = Assert.Throws<SubcommandCycleException>(
                () => CommandLineApplication.Execute<SelfCycledCommand>(new TestConsole(_output)));
            Assert.Equal(typeof(SelfCycledCommand), ex.ModelType);
        }

        [Subcommand(typeof(NamedSubCommand))]
        private class RootProgram { }

        [Command(Name = "sub")]
        private class NamedSubCommand { }

        [Fact]
        public void CommandAttributeNameIsAttachedToSubCommand()
        {
            var app = new CommandLineApplication<RootProgram>();
            app.Conventions.UseDefaultConventions();

            var command = Assert.Single(app.Commands);
            Assert.Equal("sub", command.Name);
        }
    }
}
