// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            public string First { get; }

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
            public string[] Words { get; }

            [Argument(1)]
            public string[] MoreWords { get; }
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
    }
}
