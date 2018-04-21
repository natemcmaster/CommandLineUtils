// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils.Conventions;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class DefaultHelpOptionTests
    {
        [HelpOption("-H")]
        private class ProgramWithHelpOption { }

        [Fact]
        public void ItDoesNotOverrideHelpOptionsAlreadySet()
        {
            var app = new CommandLineApplication<ProgramWithHelpOption>();
            app.Conventions.UseDefaultConventions();
            Assert.Equal("H", app.OptionHelp.ShortName);
            Assert.Null(app.OptionHelp.LongName);
        }

        [Fact]
        public void ItDoesNotAddHelpOptionIfThereAreConflicts()
        {
            var app = new CommandLineApplication();
            app.Option(DefaultHelpOptionConvention.DefaultHelpTemplate, "test", CommandOptionType.NoValue);
            app.Conventions.UseDefaultHelpOption();
            Assert.Null(app.OptionHelp);
        }

        [Fact]
        public void ItTrimsOptionsAlreadyInUse()
        {
            var app = new CommandLineApplication();
            app.Option("-h", "test", CommandOptionType.NoValue);
            app.Conventions.UseDefaultHelpOption();
            Assert.NotNull(app.OptionHelp);
            Assert.Null(app.OptionHelp.ShortName);
            Assert.NotNull(app.OptionHelp.LongName);
            Assert.NotNull(app.OptionHelp.SymbolName);
        }

        [Fact]
        public void ItTrimsOptionsAlreadyInUseOnParent()
        {
            var app = new CommandLineApplication();
            app.Option("-h", "test", CommandOptionType.NoValue, inherited: true);
            var subcmd = app.Command("sub", c =>
            {
                c.Conventions.UseDefaultHelpOption();
            });

            Assert.Null(app.OptionHelp);
            Assert.NotNull(subcmd.OptionHelp);
            Assert.Null(subcmd.OptionHelp.ShortName);
            Assert.NotNull(subcmd.OptionHelp.LongName);
            Assert.NotNull(subcmd.OptionHelp.SymbolName);
        }

        [Subcommand("sub", typeof(Sub))]
        private class Program
        {
            public class Sub { }
        }

        [Fact]
        public void ItAddsHelpOption()
        {
            var app = new CommandLineApplication<Program>();
            app.Conventions.UseDefaultConventions();
            var subcmd = Assert.Single(app.Commands);
            Assert.NotNull(app.OptionHelp);
            Assert.False(app.OptionHelp.Inherited);
            Assert.Equal(DefaultHelpOptionConvention.DefaultHelpTemplate, app.OptionHelp.Template);
            Assert.NotSame(app.OptionHelp, subcmd.OptionHelp);
        }

        [SuppressDefaultHelpOption]
        private class ProgramSuppressedHelp
        {
        }

        [Fact]
        public void ItCanBeSuppressed()
        {
            var app = new CommandLineApplication<ProgramSuppressedHelp>();
            app.Conventions.UseDefaultConventions();
            Assert.Null(app.OptionHelp);
        }
    }
}
