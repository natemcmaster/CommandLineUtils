// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ValidateMethodConventionTests
    {
        private class ProgramWithValidate
        {
            private ValidationResult OnValidate(ValidationContext context, CommandLineContext appContext)
            {
                return new ValidationResult("Failed");
            }
        }

        [Fact]
        public void ValidatorAddedViaConvention()
        {
            var app = new CommandLineApplication<ProgramWithValidate>();
            app.Conventions.UseOnValidateMethodFromModel();
            var result = app.GetValidationResult();
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Failed", result.ErrorMessage);
        }

        private class ProgramWithBadOnValidate
        {
            private void OnValidate() { }
        }

        [Fact]
        public void ConventionThrowsIfOnValidateDoesNotReturnValidationresult()
        {
            var app = new CommandLineApplication<ProgramWithBadOnValidate>();
            var ex = Assert.Throws<InvalidOperationException>(() => app.Conventions.UseOnValidateMethodFromModel());
            Assert.Equal(Strings.InvalidOnValidateReturnType(typeof(ProgramWithBadOnValidate)), ex.Message);
        }


        [Command]
        [Subcommand(typeof(SubcommandValidate))]
        private class MainValidate
        {
            [Option]
            public int? Middle { get; }

            private ValidationResult OnValidate(ValidationContext context, CommandLineContext appContext)
            {
                if (this.Middle.HasValue && this.Middle < 0)
                {
                    return new ValidationResult("Middle must be greater than 0");
                }

                Assert.Equal(typeof(CommandLineApplication<MainValidate>), context.ObjectInstance.GetType());

                return ValidationResult.Success;
            }
        }

        [Command("subcommand")]
        private class SubcommandValidate
        {
            [Option]
            public int Start { get; private set; } = 0;

            [Option]
            public int End { get; private set; } = Int32.MaxValue;

            private ValidationResult OnValidate(ValidationContext context, CommandLineContext appContext)
            {
                if (this.Start >= this.End)
                {
                    return new ValidationResult("End must be greater than start");
                }

                Assert.Equal(typeof(CommandLineApplication<SubcommandValidate>), context.ObjectInstance.GetType());
                var subcommand = (CommandLineApplication<SubcommandValidate>) context.ObjectInstance;
                var main = (CommandLineApplication<MainValidate>?) subcommand.Parent;

                var middle = main?.Model.Middle;
                if (middle.HasValue)
                {
                    if (middle.Value < this.Start || middle.Value >= this.End)
                    {
                        return new ValidationResult("Middle must be between start and end");
                    }
                }

                return ValidationResult.Success;
            }
        }

        [Theory]
        [InlineData("subcommand -s 999 -e 123", "End must be greater than start")]
        [InlineData("-m 999 subcommand -s 111 -e 123", "Middle must be between start and end")]
        [InlineData("-m -5 subcommand -s -100 -e 100", "Middle must be greater than 0")]
        [InlineData("-m 111 subcommand", null)]
        [InlineData("subcommand -s 111 -e 123", null)]
        [InlineData("-m 111 subcommand -s 100 -e 123", null)]
        public void ValidatorShouldGetDeserailizedModelInSubcommands(string args, string error)
        {
            var app = new CommandLineApplication<MainValidate>();
            app.Conventions.UseDefaultConventions();

            // this tests that the model is actually given values before it passed to command validation
            var parseResult = app.Parse(args.Split(' '));

            var result = parseResult.SelectedCommand.GetValidationResult();

            if (error == null)
            {
                Assert.Equal(ValidationResult.Success, result);
            }
            else
            {
                Assert.NotEqual(ValidationResult.Success, result);
                Assert.Equal(error, result.ErrorMessage);
            }
        }
    }
}
