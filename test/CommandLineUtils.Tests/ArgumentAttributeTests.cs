// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
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
    }
}
