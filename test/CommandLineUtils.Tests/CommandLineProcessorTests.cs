// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandLineProcessorTests
    {
        private readonly ITestOutputHelper _output;

        public CommandLineProcessorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("--log", null)]
        [InlineData("--log:", "")]
        [InlineData("--log: ", " ")]
        [InlineData("--log:verbose", "verbose")]
        [InlineData("--log=verbose", "verbose")]
        public void CanParseSingleOrNoValueParameter(string input, string expected)
        {
            var app = new CommandLineApplication();
            var opt = app.Option("--log", "Log level", CommandOptionType.SingleOrNoValue);
            app.Parse(input);
            Assert.True(opt.HasValue(), "Option should have value");
            Assert.Equal(expected, opt.Value());
            Assert.Empty(app.RemainingArguments);
        }

        [Theory]
        [InlineData(new []{ "--param1"}, null, null)]
        [InlineData(new []{ "--param1", "--param2", "p2"}, null, "p2")]
        [InlineData(new []{ "--param1:p1", "--param2", "p2"}, "p1", "p2")]
        public void CanParseSingleOrNoValueParameters(string[] args, string param1, string param2)
        {
            var app = new CommandLineApplication();
            var opt1 = app.Option("--param1", "param1", CommandOptionType.SingleOrNoValue);
            var opt2 = app.Option("--param2", "param2", CommandOptionType.SingleValue);
            app.Parse(args);
            Assert.Equal(param1, opt1.Value());
            Assert.Equal(param2, opt2.Value());
            Assert.Empty(app.RemainingArguments);
        }

        [Fact]
        public void ThrowsWhenSingleValueIsNotProvided()
        {
            var app = new CommandLineApplication();
            app.Option("--log", "Log level", CommandOptionType.SingleValue);
            Assert.Throws<CommandParsingException>(() => app.Parse("--log" ));
        }
    }
}
