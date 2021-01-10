// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandArgumentTests
    {

        [Fact]
        public void AlwaysHasDefaultValueForValueTypes()
        {
            var app = new CommandLineApplication();
            var argument = app.Argument<int>("abc", "xyz");
            Assert.Equal(default, argument.DefaultValue);
            Assert.True(argument.HasValue);
        }

        [Fact]
        public void DoesNotHaveDefaultForReferenceTypes()
        {
            var app = new CommandLineApplication();
            var argument = app.Argument<int?>("abc", "xyz");
            Assert.Null(argument.DefaultValue);
            Assert.False(argument.HasValue);
        }

        [Fact]
        public void DefaultValueReturnedIfUnset()
        {
            var option = new CommandArgument
            {
                DefaultValue = "ABC"
            };

            Assert.Equal("ABC", option.Value);
            Assert.Equal(new List<string> { "ABC" }, option.Values);
        }

        [Fact]
        public void DefaultValueOfTReturnedIfUnset()
        {
            var option = new CommandArgument<int>(StockValueParsers.Int32)
            {
                DefaultValue = 42
            };

            Assert.Equal("42", option.Value);
            Assert.Equal(new List<string> { "42" }, option.Values);
            Assert.Equal(42, option.ParsedValue);
            Assert.Equal(new List<int> { 42 }, option.ParsedValues);
        }

        [Fact]
        public void DefaultOverriddenBySettingValue()
        {
            var option = new CommandArgument
            {
                DefaultValue = "ABC"
            };

            option.TryParse("xyz");

            Assert.Equal("xyz", option.Value);
        }

        [Fact]
        public void DefaultOfTOverriddenBySettingValue()
        {
            var option = new CommandArgument<int>(StockValueParsers.Int32)
            {
                DefaultValue = 42
            };

            option.TryParse("999");

            Assert.Equal("999", option.Value);
            Assert.Equal(new List<string> { "999" }, option.Values);
            Assert.Equal(999, option.ParsedValue);
            Assert.Equal(new List<int> { 999 }, option.ParsedValues);
        }

        [Fact]
        public void DefaultOverriddenAndResetReturnsDefault()
        {
            var option = new CommandArgument
            {
                DefaultValue = "ABC"
            };

            option.TryParse("xyz");

            option.Reset();

            Assert.Equal("ABC", option.Value);
        }
    }
}
