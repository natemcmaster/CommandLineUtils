// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ParsedMethodConventionTests
    {
        private class ProgramWithParsed
        {
            private void OnParsed(ParseResult context, CommandLineContext appContext)
            {
                ParsedMethodCalled = true;
            }

            public bool ParsedMethodCalled { get; private set; } = false;
        }

        [Fact]
        public void ParsedAddedViaConvention()
        {
            var app = new CommandLineApplication<ProgramWithParsed>();
            app.Conventions.UseOnParsedMethodFromModel();
            var castApp = ((CommandLineApplication<ProgramWithParsed>) app.Parse().SelectedCommand);
            Assert.True(castApp.Model.ParsedMethodCalled);
        }

        private class ProgramWithBadOnParsed
        {
            private Task OnParsed() => Task.CompletedTask;
        }

        [Fact]
        public void ConventionThrowsIfOnParsedReturnsAnything()
        {
            var app = new CommandLineApplication<ProgramWithBadOnParsed>();
            var ex = Assert.Throws<InvalidOperationException>(() => app.Conventions.UseOnParsedMethodFromModel());
            Assert.Equal(Strings.InvalidOnParsedReturnType(typeof(ProgramWithBadOnParsed)), ex.Message);
        }


        [Command]
        [Subcommand("subcommand", typeof(SubcommandParsed))]
        private class MainParsed
        {
            private void OnParsed(ParseResult context, CommandLineContext appContext)
            {
                SetByOnParsed = true;
            }

            public bool SetByOnParsed { get; private set; } = false;
        }

        [Command]
        private class SubcommandParsed
        {
            private void OnParsed()
            {
                SetByOnParsed = true;
            }

            public bool SetByOnParsed { get; private set; } = false;
        }

        [Fact]
        public void ParsedCommandsShouldWorkRecursively()
        {
            var app = new CommandLineApplication<MainParsed>();
            app.Conventions.UseDefaultConventions();

            var parseResult = app.Parse("subcommand");

            var result = parseResult.SelectedCommand.GetValidationResult();

            Assert.True(((CommandLineApplication<MainParsed>) parseResult.SelectedCommand.Parent).Model.SetByOnParsed);
            Assert.True(((CommandLineApplication<SubcommandParsed>) parseResult.SelectedCommand.Parent.Commands.First()).Model.SetByOnParsed);
        }
    }
}
