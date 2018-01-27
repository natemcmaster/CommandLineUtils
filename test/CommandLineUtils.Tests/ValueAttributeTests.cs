// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ValueAttributeTests
    {
        private readonly ITestOutputHelper _output;

        public ValueAttributeTests(ITestOutputHelper output)
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


        private enum Color
        {
            Red,
            Blue,
            Green,
        }

        private class EnumProgram
        {
            [Argument(0)]
            public Color Option { get; }

            private void OnExecute() { }
        }

        [Theory]
        [InlineData(nameof(Color.Red), true)]
        [InlineData(nameof(Color.Green), true)]
        [InlineData(nameof(Color.Blue), true)]
        [InlineData("yellow", false)]
        [InlineData("red", false)]
        [InlineData("RED", false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public void ValidatesEnum(string value, bool isValid)
        {
            var exitCode = isValid ? 0 : 1;
            var console = new TestConsole(_output);
            var app = new CommandLineApplication(console);
            app.Argument("v", "v").Accepts().Enum<Color>();
            Assert.Equal(exitCode, app.Execute(value));
        }

        [Theory]
        [InlineData("red")]
        [InlineData("RED")]
        public void ValidatesEnumIgnoreCase(string value)
        {
            var console = new TestConsole(_output);
            var app = new CommandLineApplication(console);
            app.Argument("v", "v").Accepts().Enum<Color>(ignoreCase: true);
            Assert.Equal(0, app.Execute(value));
        }

        [Theory]
        [InlineData(nameof(Color.Red), true)]
        // when using attribute binding, case-insensitivity is default since the raw value
        // is hidden from the user anyways
        [InlineData("red", true)]
        [InlineData("RED", true)]
        [InlineData(nameof(Color.Green), true)]
        [InlineData(nameof(Color.Blue), true)]
        [InlineData("yellow", false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public void ValidatesEnumAsParameterType(string value, bool isValid)
        {
            var exitCode = isValid ? 0 : 1;
            var console = new TestConsole(_output);
            Assert.Equal(exitCode, CommandLineApplication.Execute<EnumProgram>(console, value));
        }
    }
}
