// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ArgumentAttributeTests
    {
        private readonly ITestOutputHelper _output;

        public ArgumentAttributeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private CommandLineApplication<T> Create<T>()
            where T : class
        {
            var app = new CommandLineApplication<T>(new TestConsole(_output));
            app.Conventions.UseArgumentAttributes();
            return app;
        }

        private class DuplicateArguments
        {
            [Argument(0)]
            public string? First { get; }

            [Argument(0)]
            public int AlsoFirst { get; }
        }

        [Fact]
        public void ThrowsWhenDuplicateArgumentPositionsAreSpecified()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Create<DuplicateArguments>());

            Assert.Equal(
                Strings.DuplicateArgumentPosition(
                    0,
                    typeof(DuplicateArguments).GetProperty("AlsoFirst"),
                    typeof(DuplicateArguments).GetProperty("First")),
                ex.Message);
        }

        private class MultipleValuesMultipleArgs
        {
            [Argument(0)]
            public string[]? Words { get; }

            [Argument(1)]
            public string[]? MoreWords { get; }
        }

        [Fact]
        public void ThrowsWhenMultipleArgumentsAllowMultipleValues()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Create<MultipleValuesMultipleArgs>());

            Assert.Equal(
                Strings.OnlyLastArgumentCanAllowMultipleValues("Words"),
                ex.Message);
        }

        private class ArgHasDefaultValues
        {
            [Argument(0)]
            public string Arg1 { get; } = "a";

            [Argument(1)]
            public string[] Arg2 { get; } = new[] { "b", "c" };
        }

        [Fact]
        public void KeepsDefaultValues()
        {
            var app1 = Create<ArgHasDefaultValues>();
            app1.Parse("z", "y");
            Assert.Equal("z", app1.Model.Arg1);
            Assert.Equal(new[] { "y" }, app1.Model.Arg2);

            var app2 = Create<ArgHasDefaultValues>();
            app2.Parse("z");
            Assert.Equal("z", app2.Model.Arg1);
            Assert.Equal(new[] { "b", "c" }, app2.Model.Arg2);

            var app3 = Create<ArgHasDefaultValues>();
            app3.Parse();
            Assert.Equal("a", app3.Model.Arg1);
            Assert.Equal(new[] { "b", "c" }, app3.Model.Arg2);
        }

        [Subcommand(typeof(ACommand))]
        public class Program
        {
        }

        [Command("a")]
        [Subcommand(typeof(BCommand))]
        public class ACommand
        {
            [Argument(0)]
            public string? Arg1 { get; set; }
        }

        [Command("b")]
        public class BCommand
        {
            [Argument(0)]
            public string? Arg1 { get; set; }
        }

        [Fact]
        public void SameArgumentInSubcommandsCallingACommand()
        {
            var app1 = new CommandLineApplication<Program>();
            app1.Conventions.UseDefaultConventions();
            var result = app1.Parse("a", "any-value");
            var command = result.SelectedCommand as CommandLineApplication<ACommand>;
            Assert.NotNull(command);
            Assert.Equal("any-value", command.Model.Arg1);
        }

        [Fact]
        public void SameArgumentInSubcommandsCallingBCommand()
        {
            var app1 = new CommandLineApplication<Program>();
            app1.Conventions.UseDefaultConventions();
            var result = app1.Parse("a", "b", "any-value");
            var command = result.SelectedCommand as CommandLineApplication<BCommand>;
            Assert.NotNull(command);
            Assert.Equal("any-value", command.Model.Arg1);
        }
    }
}
