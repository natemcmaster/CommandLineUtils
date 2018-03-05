// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class RemainingArgsPropertyConventionTests : ConventionTestBase
    {
        public RemainingArgsPropertyConventionTests(ITestOutputHelper output) : base(output)
        {
        }

        protected CommandLineApplication<T> Create<T>() where T : class
        {
            var app = Create<T, RemainingArgsPropertyConvention>();
            app.ThrowOnUnexpectedArgument = false;
            return app;
        }

        private class RemainingArguments_Array
        {
            public string[] RemainingArguments { get; }
        }

        [Fact]
        public void ItSetsRemainingArguments_Array()
        {
            var app = Create<RemainingArguments_Array>();
            app.Parse("a", "b");
            Assert.Equal(new[] { "a", "b" }, app.Model.RemainingArguments);
        }

        private class RemainingArgs_Array
        {
            public string[] RemainingArgs { get; }
        }


        [Fact]
        public void ItSetsRemainingArgs_Array()
        {
            var app = Create<RemainingArgs_Array>();
            app.Parse("a", "b");
            Assert.Equal(new[] { "a", "b" }, app.Model.RemainingArgs);
        }

        private class Parent
        {
            public object Subcommand { get; }
        }

        [Fact]
        public void ItSetsRemainingArgsOnSubcommand()
        {
            var app = Create<Parent>();
            app.Command<RemainingArgs_Array>("subcmd", _ => { }, throwOnUnexpectedArg: false);
            var result = app.Parse("subcmd", "a", "b");
            var subcmd = Assert.IsType<CommandLineApplication<RemainingArgs_Array>>(result.SelectedCommand);
            Assert.Equal(new[] { "a", "b" }, subcmd.Model.RemainingArgs);
        }

        private class RemainingArgs_List
        {
            public List<string> RemainingArguments { get; }
        }

        [Fact]
        public void ItSetsRemainingArguments_List()
        {
            var app = new CommandLineApplication<RemainingArgs_List>(false);
            app.Conventions.UseDefaultConventions();
            app.Parse("a", "b");
            Assert.Equal(new[] { "a", "b" }, app.Model.RemainingArguments);
        }
    }
}
