// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class HelpOptionTests
    {
        private readonly ITestOutputHelper _output;

        public HelpOptionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class NoHelpOptionClass
        {
            [Option]
            public int OptionA { get; set; }
        }

        [Fact]
        public void DoesNotAddHelpOptionByDefault()
        {
            var app = new CommandLineApplication<NoHelpOptionClass>();
            app.Conventions.UseHelpOptionAttribute();
            Assert.Null(app.OptionHelp);
        }

        private class MultipleHelpOptions
        {
            [HelpOption("-h1")]
            public bool IsHelp1 { get; set; }

            [HelpOption("-h2")]
            public bool IsHelp2 { get; set; }
        }

        [Fact]
        public void ThrowsWhenMultipleHelpOptionsInType()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new CommandLineApplication<MultipleHelpOptions>().Conventions.UseHelpOptionAttribute());
            Assert.Equal(Strings.MultipleHelpOptionPropertiesFound, ex.Message);
        }

        [HelpOption]
        private class HelpOptionOnTypeAndProp
        {
            [HelpOption]
            public bool IsHelp { get; set; }
        }

        [Fact]
        public void ThrowsWhenMultipleHelpOptionUsedOnTypeAndProperti()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new CommandLineApplication<HelpOptionOnTypeAndProp>().Conventions.UseHelpOptionAttribute());
            Assert.Equal(Strings.HelpOptionOnTypeAndProperty, ex.Message);
        }

        private class HelpOptionOnNonBoolean
        {
            [HelpOption]
            public string? IsHelpOption { get; set; }
        }

        [Fact]
        public void ThrowsIfHelpOptionPropIsNotBool()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new CommandLineApplication<HelpOptionOnNonBoolean>().Conventions.UseHelpOptionAttribute());
            Assert.Equal(Strings.NoValueTypesMustBeBoolean, ex.Message);
        }

        private class DuplicateOptionAttributes
        {
            [HelpOption]
            [Option]
            public string? IsHelpOption { get; set; }
        }

        [Fact]
        public void ThrowsIfMultipleAttributesApplied()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new CommandLineApplication<DuplicateOptionAttributes>().Conventions.UseHelpOptionAttribute());
            var prop = typeof(DuplicateOptionAttributes).GetProperty(nameof(DuplicateOptionAttributes.IsHelpOption));
            Assert.Equal(Strings.BothOptionAndHelpOptionAttributesCannotBeSpecified(prop), ex.Message);
        }

        [HelpOption("-h|--help", Description = "My help info")]
        private class WithTypeHelpOption
        {
        }

        [Fact]
        public void SetsHelpOptionOnType()
        {
            var app = new CommandLineApplication<WithTypeHelpOption>();
            app.Conventions.UseHelpOptionAttribute();
            Assert.NotNull(app.OptionHelp);
            Assert.Equal(CommandOptionType.NoValue, app.OptionHelp?.OptionType);
            Assert.Null(app.OptionHelp?.SymbolName);
            Assert.Equal("h", app.OptionHelp?.ShortName);
            Assert.Equal("help", app.OptionHelp?.LongName);
            Assert.Equal("My help info", app.OptionHelp?.Description);
        }

        private class WithPropHelpOption
        {
            [HelpOption("-h|--help", Description = "My help info")]
            public bool IsHelp { get; }
        }

        [Fact]
        public void SetsHelpOptionOnProp()
        {
            var app = new CommandLineApplication<WithPropHelpOption>();
            app.Conventions.UseHelpOptionAttribute();
            Assert.NotNull(app.OptionHelp);
            Assert.Equal(CommandOptionType.NoValue, app.OptionHelp?.OptionType);
            Assert.Null(app.OptionHelp?.SymbolName);
            Assert.Equal("h", app.OptionHelp?.ShortName);
            Assert.Equal("help", app.OptionHelp?.LongName);
            Assert.Equal("My help info", app.OptionHelp?.Description);
        }

        [HelpOption]
        private class SimpleHelpApp
        {
            private void OnExecute()
            {
                throw new InvalidOperationException("This method should not be invoked");
            }
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("-?")]
        [InlineData("--help")]
        public void OnExecuteIsNotInvokedWhenHelpOptionSpecified(string arg)
        {
            Assert.Equal(0, CommandLineApplication.Execute<SimpleHelpApp>(new TestConsole(_output), arg));
        }

        [Command(Name = "lvl1")]
        [HelpOption(Inherited = true)]
        [Subcommand(typeof(Sub))]
        private class Parent
        {
            [Command("lvl2")]
            private class Sub
            {
                [Argument(0, Name = "lvl-arg", Description = "subcommand argument")]
                public string? Arg { get; }
            }
        }

        [Fact]
        public void HelpOptionIsInherited()
        {
            var sb = new StringBuilder();
            var outWriter = new StringWriter(sb);
            var app = new CommandLineApplication<Parent> { Out = outWriter };
            app.Conventions.UseDefaultConventions();
            app.Commands.ForEach(f => f.Out = outWriter);
            app.Execute("lvl2", "--help");
            var outData = sb.ToString();

            Assert.True(app.OptionHelp?.HasValue());
            Assert.Contains("Usage: lvl1 lvl2 [options] <lvl-arg>", outData);
        }

        [Theory]
        [InlineData(new[] { "get", "--help" }, "Usage: updater get [options]")]
        [InlineData(new[] { "get", "-h" }, "Usage: updater get [options]")]
        [InlineData(new[] { "get", "-?" }, "Usage: updater get [options]")]
        [InlineData(new[] { "--help" }, "Usage: updater [command] [options]")]
        [InlineData(new[] { "-h" }, "Usage: updater [command] [options]")]
        [InlineData(new[] { "-?" }, "Usage: updater [command] [options]")]
        public void NestedHelpOptionsChoosesHelpOptionNearestSelectedCommand(string[] args, string helpNeedle)
        {
            var sb = new StringBuilder();
            var outWriter = new StringWriter(sb);

            var app = new CommandLineApplication { Name = "updater", Out = outWriter };
            app.HelpOption(true);

            app.Command("get", getCommand =>
            {
                getCommand.Description = "Gets a list of things.";
                getCommand.HelpOption();

                getCommand.OnExecute(() =>
                {
                    getCommand.ShowHelp();
                    return 1;
                });
            });

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 1;
            });

            app.Commands.ForEach(f => f.Out = outWriter);

            app.Execute(args);
            var outData = sb.ToString();

            Assert.Contains(helpNeedle, outData);
        }
    }
}
