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
        public void DoesNotHaveDefaultForValueTypes()
        {
            var app = new CommandLineApplication();
            var arg = app.Argument<int>("abc", "xyz");
            Assert.False(arg.HasValue);
            Assert.Null(arg.Value);
        }

        [Fact]
        public void DoesNotHaveDefaultForReferenceTypes()
        {
            var app = new CommandLineApplication();
            var arg = app.Argument<int?>("abc", "xyz");
            Assert.Null(arg.DefaultValue);
            Assert.False(arg.HasValue);
        }

        [Fact]
        public void DefaultValueReturnedIfUnset()
        {
            var arg = new CommandArgument
            {
                DefaultValue = "ABC"
            };

            Assert.Equal("ABC", arg.Value);
            Assert.True(arg.HasValue);
            Assert.Equal(new List<string> { "ABC" }, arg.Values);
        }

        [Fact]
        public void DefaultValueOfTReturnedIfUnset()
        {
            var arg = new CommandArgument<int>(StockValueParsers.Int32)
            {
                DefaultValue = 42
            };

            Assert.Equal("42", arg.Value);
            Assert.True(arg.HasValue);
            Assert.Equal(new List<string> { "42" }, arg.Values);
            Assert.Equal(42, arg.ParsedValue);
            Assert.Equal(new List<int> { 42 }, arg.ParsedValues);
        }

        [Fact]
        public void DefaultOverriddenBySettingValue()
        {
            var arg = new CommandArgument
            {
                DefaultValue = "ABC"
            };

            arg.TryParse("xyz");

            Assert.Equal("xyz", arg.Value);
        }

        [Fact]
        public void DefaultOfTOverriddenBySettingValue()
        {
            var arg = new CommandArgument<int>(StockValueParsers.Int32)
            {
                DefaultValue = 42
            };

            arg.TryParse("999");

            Assert.Equal("999", arg.Value);
            Assert.True(arg.HasValue);
            Assert.Equal(new List<string> { "999" }, arg.Values);
            Assert.Equal(999, arg.ParsedValue);
            Assert.Equal(new List<int> { 999 }, arg.ParsedValues);
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
