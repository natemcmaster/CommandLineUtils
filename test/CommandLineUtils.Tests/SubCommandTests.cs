// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class SubcommandTests
    {
        private readonly ITestOutputHelper _output;

        public SubcommandTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Command(StopParsingAfterHelpOption = false)]
        [Subcommand("level1", typeof(Level1Cmd))]
        private class MasterApp : CommandBase
        {
            [Option(Inherited = true)]
            public bool Verbose { get; set; }

            protected override int OnExecute(CommandLineApplication app) => IsHelp ? 100 : 0;
        }

        [Subcommand("level2", typeof(Level2Cmd))]
        private class Level1Cmd : CommandBase
        {
            [Option("--mid")]
            public bool Mid { get; }

            protected override int OnExecute(CommandLineApplication app) => IsHelp ? 101 : 1;

            public MasterApp Parent { get; }
        }

        private class Level2Cmd : CommandBase
        {
            [Option("--value")]
            public int? Value { get; set; }

            protected override int OnExecute(CommandLineApplication app) =>
                IsHelp
                ? 102
                : Value.HasValue ? Value.Value : 2;

            public Level1Cmd Parent { get; }
        }

        abstract class CommandBase
        {
            [HelpOption("--help")]
            protected bool IsHelp { get; }

            public CommandBase Subcommand { get; set; }

            protected abstract int OnExecute(CommandLineApplication app);
        }

        [Fact]
        public void ItInvokesExecuteOnSubcommand_Bottom()
        {
            var rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output), "level1", "level2", "--value", "6");
            Assert.Equal(6, rc);
        }

        [Fact]
        public void ItInvokesExecuteOnSubcommand_Middle()
        {
            var rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output), "level1");
            Assert.Equal(1, rc);
        }

        [Fact]
        public void ItInvokesExecuteOnSubcommand_Top()
        {
            var rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output));
            Assert.Equal(0, rc);
        }

        [Fact]
        public void HandlesHelp_Bottom()
        {
            var rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output), "level1", "level2", "--help");
            Assert.Equal(102, rc);
        }

        [Fact]
        public void HandlesHelp_Middle()
        {
            var rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output), "level1", "--help");
            Assert.Equal(101, rc);
        }

        [Fact]
        public void HandlesHelp_Top()
        {
            var rc = CommandLineApplication.Execute<MasterApp>(new TestConsole(_output), "--help");
            Assert.Equal(100, rc);
        }

        [Fact]
        public void ItCreatesNestedSubCommands()
        {
            var builder = new ReflectionAppBuilder<MasterApp>();
            builder.Initialize();
            var lvl1 = Assert.Single(builder.App.Commands);
            Assert.Equal("level1", lvl1.Name);
            var lvl2 = Assert.Single(lvl1.Commands);
            Assert.Equal("level2", lvl2.Name);
        }

        [Fact]
        public void ItBindsOptionsOnParentItems()
        {
            var app = CommandLineParser.ParseArgs<MasterApp>("level1", "--mid", "level2", "--verbose", "--value", "6");
            Assert.IsType<Level1Cmd>(app.Subcommand);
            var sub = Assert.IsType<Level2Cmd>(app.Subcommand.Subcommand);
            Assert.NotNull(sub.Parent);
            Assert.NotNull(sub.Parent.Parent);
            Assert.True(sub.Parent.Mid);
            Assert.True(sub.Parent.Parent.Verbose);
            Assert.Equal(6, sub.Value);
        }
    }
}
