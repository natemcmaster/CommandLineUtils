// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandOptionTests
    {
        [Theory]
        [InlineData("-a", "a", null, null, null)]
        [InlineData("-abc", "abc", null, null, null)]
        [InlineData("-a|--name", "a", null, "name", null)]
        [InlineData("-a|--name <VALUE>", "a", null, "name", "VALUE")]
        [InlineData("-a|--name <VALUE> <VALUE2>", "a", null, "name", "VALUE2")]
        [InlineData("-a <VALUE>", "a", null, null, "VALUE")]
        [InlineData("-?|-a", "a", "?", null, null)]
        [InlineData("-?|-a|--name", "a", "?", "name", null)]
        [InlineData("-?|-a|--name <VALUE>", "a", "?", "name", "VALUE")]
        [InlineData("-?|-a <VALUE>", "a", "?", null, "VALUE")]
        [InlineData("-? <VALUE>", null, "?", null, "VALUE")]
        [InlineData("-a --name", "a", null, "name", null)]
        [InlineData("-a --name <VALUE>", "a", null, "name", "VALUE")]
        [InlineData("-a -? --name <VALUE>", "a", "?", "name", "VALUE")]
        [InlineData("--name:<VALUE>", null, null, "name", "VALUE")]
        [InlineData("--name=<VALUE>", null, null, "name", "VALUE")]
        [InlineData("-a:<VALUE> --name:<VALUE>", "a", null, "name", "VALUE")]
        [InlineData("-a=<VALUE> --name=<VALUE>", "a", null, "name", "VALUE")]
        public void ItParsesSingleValueTemplate(string template, string shortName, string symbolName, string longName, string valueName)
        {
            var opt = new CommandOption(template, CommandOptionType.SingleValue);
            Assert.Equal(shortName, opt.ShortName);
            Assert.Equal(symbolName, opt.SymbolName);
            Assert.Equal(longName, opt.LongName);
            Assert.Equal(valueName, opt.ValueName);
        }

        [Theory]
        [InlineData("--name[:<VALUE>]", null, null, "name", "VALUE")]
        [InlineData("--name[=<VALUE>]", null, null, "name", "VALUE")]
        public void ItParsesSingleOrNoValueTemplate(string template, string shortName, string symbolName, string longName, string valueName)
        {
            var opt = new CommandOption(template, CommandOptionType.SingleOrNoValue);
            Assert.Equal(shortName, opt.ShortName);
            Assert.Equal(symbolName, opt.SymbolName);
            Assert.Equal(longName, opt.LongName);
            Assert.Equal(valueName, opt.ValueName);
        }

        [Fact]
        public void DoesNotHaveDefaultForValueTypes()
        {
            var app = new CommandLineApplication();
            var option = app.Option<int>("--value <ABC>", "abc", CommandOptionType.SingleValue);
            Assert.False(option.HasValue());
            Assert.Null(option.Value());
        }

        [Fact]
        public void DoesNotHaveDefaultForReferenceTypes()
        {
            var app = new CommandLineApplication();
            var option = app.Option<int?>("--value <ABC>", "abc", CommandOptionType.SingleValue);
            Assert.Null(option.DefaultValue);
            Assert.False(option.HasValue());
        }

        [Fact]
        public void DefaultValueReturnedIfUnset()
        {
            var option = new CommandOption("--value <ABC>", CommandOptionType.SingleValue)
            {
                DefaultValue = "ABC"
            };

            Assert.True(option.HasValue());
            Assert.Equal("ABC", option.Value());
            Assert.Equal(new List<string> { "ABC" }, option.Values);
        }

        [Fact]
        public void DefaultValueOfTReturnedIfUnset()
        {
            var option = new CommandOption<int>(StockValueParsers.Int32, "--value <ABC>", CommandOptionType.SingleValue)
            {
                DefaultValue = 42
            };

            Assert.True(option.HasValue());
            Assert.Equal("42", option.Value());
            Assert.Equal(new List<string> { "42" }, option.Values);
            Assert.Equal(42, option.ParsedValue);
            Assert.Equal(new List<int> { 42 }, option.ParsedValues);
        }

        [Fact]
        public void DefaultOverriddenBySettingValue()
        {
            var option = new CommandOption("--value <ABC>", CommandOptionType.SingleValue)
            {
                DefaultValue = "ABC"
            };

            option.TryParse("xyz");

            Assert.Equal("xyz", option.Value());
        }

        [Fact]
        public void DefaultOfTOverriddenBySettingValue()
        {
            var option = new CommandOption<int>(StockValueParsers.Int32, "--value <ABC>", CommandOptionType.SingleValue)
            {
                DefaultValue = 42
            };

            option.TryParse("999");

            Assert.True(option.HasValue());
            Assert.Equal("999", option.Value());
            Assert.Equal(new List<string> { "999" }, option.Values);
            Assert.Equal(999, option.ParsedValue);
            Assert.Equal(new List<int> { 999 }, option.ParsedValues);
        }

        [Fact]
        public void DefaultOverriddenAndResetReturnsDefault()
        {
            var option = new CommandOption("--value <ABC>", CommandOptionType.SingleValue)
            {
                DefaultValue = "ABC"
            };

            option.TryParse("xyz");

            option.Reset();

            Assert.Equal("ABC", option.Value());
        }
    }
}
