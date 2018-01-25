// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ValueIsInSetAttributeTests
    {
        private readonly ITestOutputHelper _output;

        public ValueIsInSetAttributeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class Program
        {
            [Argument(0)]
            [Values("red", "blue", "green")]
            public string Option { get; }

            private void OnExecute() { }
        }

        [Theory]
        [InlineData("red", true)]
        [InlineData("green", true)]
        [InlineData("blue", true)]
        [InlineData("yellow", false)]
        [InlineData("RED", false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public void ValidatesValueInSet(string value, bool isValid)
        {
            var exitCode = isValid ? 0 : 1;
            var console = new TestConsole(_output);
            Assert.Equal(exitCode, CommandLineApplication.Execute<Program>(console, value));

            var app = new CommandLineApplication(console);
            app.Argument("v", "v").Accepts().Values("red", "blue", "green");
            Assert.Equal(exitCode, app.Execute(value));
        }

        private class IgnoreCaseProgram
        {
            [Argument(0)]
            [Values("red", "blue", "green", IgnoreCase = true)]
            public string Option { get; }

            private void OnExecute() { }
        }

        [Theory]
        [InlineData("red")]
        [InlineData("RED")]
        public void ValidatesValueInSetIgnoreCase(string value)
        {
            var console = new TestConsole(_output);
            Assert.Equal(0, CommandLineApplication.Execute<IgnoreCaseProgram>(console, value));

            var app = new CommandLineApplication(console);
            app.Argument("v", "v").Accepts().Values(/* ignoreCase */ true, "red", "blue", "green");
            Assert.Equal(0, app.Execute(value));
        }
    }
}
