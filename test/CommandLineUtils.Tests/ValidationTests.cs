// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ValidationTests
    {
        private readonly ITestOutputHelper _output;

        public ValidationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ValidationHandlerInvoked()
        {
            var app = new CommandLineApplication();
            var called = false;
            app.OnValidationError(_ => called = true);
            app.Argument("t", "test").IsRequired();
            Assert.NotEqual(0, app.Execute());
            Assert.True(called);
        }

        [Fact]
        public void ValidationHandlerOfSubcommandIsInvoked()
        {
            var app = new CommandLineApplication();
            var sub = app.Command("sub", c => { });
            var called = false;
            sub.OnValidationError(_ => called = true);
            var arg = sub.Argument<int>("t", "test").IsRequired();
            Assert.NotEqual(0, app.Execute("sub"));
            Assert.True(called, "Validation on subcommand should be called");
        }

        [Fact]
        public void ValidatorInvoked()
        {
            var app = new CommandLineApplication();
            var called = false;
            app.OnValidate(_ =>
            {
                called = true;
                return ValidationResult.Success;
            });
            Assert.Equal(0, app.Execute());
            Assert.True(called);
        }

        [Theory]
        [InlineData(CommandOptionType.NoValue, new string[0], false)]
        [InlineData(CommandOptionType.SingleValue, new string[0], false)]
        [InlineData(CommandOptionType.MultipleValue, new string[0], false)]
        [InlineData(CommandOptionType.SingleValue, new[] { "-t", "" }, false)]
        [InlineData(CommandOptionType.SingleValue, new[] { "-t", " " }, false)]
        [InlineData(CommandOptionType.SingleValue, new[] { "-t", null }, true)]
        [InlineData(CommandOptionType.SingleValue, new[] { "-t", null }, false)]
        [InlineData(CommandOptionType.MultipleValue, new[] { "-t", "val", "-t", "" }, false)]
        public void RequiredOption_Fail(CommandOptionType type, string[] args, bool allowEmptyStrings)
        {
            var app = new CommandLineApplication(new TestConsole(_output));
            app.Option("-t", "Test", type).IsRequired(allowEmptyStrings);
            Assert.NotEqual(0, app.Execute(args));
        }

        [Theory]
        [InlineData(CommandOptionType.NoValue, new[] { "-t" }, false)]
        [InlineData(CommandOptionType.SingleValue, new[] { "-t", "" }, true)]
        [InlineData(CommandOptionType.SingleValue, new[] { "-t", " " }, true)]
        [InlineData(CommandOptionType.SingleValue, new[] { "-t", "val" }, false)]
        [InlineData(CommandOptionType.MultipleValue, new[] { "-t", "val" }, false)]
        [InlineData(CommandOptionType.MultipleValue, new[] { "-t", "val", "-t", "" }, true)]
        public void RequiredOption_Pass(CommandOptionType type, string[] args, bool allowEmptyStrings)
        {
            var app = new CommandLineApplication(new TestConsole(_output));
            app.Option("-t", "Test", type).IsRequired(allowEmptyStrings);
            Assert.Equal(0, app.Execute(args));
        }

        private class RequiredOption
        {
            [Required, Option]
            public string? Param { get; set; }

            private void OnExecute() { }
        }

        [Fact]
        public void RequiredOption_Attribute_Pass()
        {
            Assert.Equal(0, CommandLineApplication.Execute<RequiredOption>("-p", "p"));
        }

        // Workaround https://github.com/dotnet/roslyn/issues/33199 https://github.com/xunit/xunit/issues/1897
#nullable disable
        [Theory]
        [InlineData(null)]
        [InlineData(new object[] { new[] { "-p", "" } })]
        [InlineData(new object[] { new[] { "-p", " " } })]
        public void RequiredOption_Attribute_Fail(string[] args)
        {
#nullable enable
            Assert.NotEqual(0, CommandLineApplication.Execute<RequiredOption>(new TestConsole(_output), args));
        }

        [Theory]
        [InlineData(new[] { "" }, false)]
        [InlineData(new string[0], false)]
        [InlineData(new[] { " " }, false)]
        [InlineData(new[] { "val", "" }, false)]
        [InlineData(new string?[] { null }, false)]
        public void RequiredArgument_Fail(string?[] args, bool allowEmptyStrings)
        {
            var app = new CommandLineApplication(new TestConsole(_output));
            app.Argument("Test", "Test arg", multipleValues: true).IsRequired(allowEmptyStrings);
            Assert.NotEqual(0, app.Execute(args!));
        }

        [Theory]
        [InlineData(new[] { "" }, true)]
        [InlineData(new[] { " " }, true)]
        [InlineData(new[] { "val" }, false)]
        [InlineData(new[] { "val", "" }, true)]
        public void RequiredArgument_Pass(string[] args, bool allowEmptyStrings)
        {
            var app = new CommandLineApplication(new TestConsole(_output));
            app.Argument("Test", "Test arg", multipleValues: true).IsRequired(allowEmptyStrings);
            Assert.Equal(0, app.Execute(args));
        }

        [Theory]
        [InlineData("-n", "-n")]
        [InlineData("-n <NAME>", "-n")]
        [InlineData("-n|--name", "--name")]
        [InlineData("-n|--name <NAME>", "--name")]
        public void RequiredOption_ErrorMessage(string template, string valueName)
        {
            var required = new RequiredAttribute();
            var expected = required.FormatErrorMessage(valueName);
            var app = new CommandLineApplication();
            app.Option(template, string.Empty, CommandOptionType.SingleValue).IsRequired();
            var validation = app.GetValidationResult();
            Assert.NotNull(validation);
            Assert.Equal(expected, validation!.ErrorMessage);
        }

        [Fact]
        public void RequiredArgument_ErrorMessage()
        {
            var required = new RequiredAttribute();
            var expected = required.FormatErrorMessage("Arg");
            var app = new CommandLineApplication();
            app.Argument("Arg", string.Empty).IsRequired();
            var validation = app.GetValidationResult();
            Assert.NotNull(validation);
            Assert.Equal(expected, validation!.ErrorMessage);
        }

        [Subcommand(typeof(ValidationErrorSubcommand))]
        private class ValidationErrorApp
        {
            [Required, Option]
            public string? Name { get; }
            private int OnValidationError()
            {
                return 7;
            }
        }

        [Command("sub")]
        private class ValidationErrorSubcommand
        {
            [Argument(0), Required]
            public string[]? Args { get; }

            private int OnValidationError()
            {
                return 49;
            }
        }

        [Fact]
        public void OnValidationErrorIsInvokedOnError()
        {
            Assert.Equal(7, CommandLineApplication.Execute<ValidationErrorApp>(new TestConsole(_output)));
        }

        [Fact]
        public void OnValidationErrorIsInvokedOnSubcommandError()
        {
            Assert.Equal(49, CommandLineApplication.Execute<ValidationErrorApp>(new TestConsole(_output), "sub"));
        }

        private class ThrowOnExecuteApp
        {
            [Option, Required]
            public string? Name { get; }
            private int OnExecute()
            {
                throw new InvalidOperationException("This method should not be invoked");
            }
        }

        [Fact]
        public void OnExecuteIsNotInvokedOnValidationError()
        {
            Assert.Equal(1, CommandLineApplication.Execute<ThrowOnExecuteApp>(new TestConsole(_output)));
        }

        [Fact]
        public void ValidationErrorOnlyCalledOnSubcommand()
        {
            var app = new CommandLineApplication();
            app.OnValidationError(_ => throw new InvalidOperationException());
            var cmd = app.Command("sub", c => { });
            cmd.Option("-a --all", "all", CommandOptionType.NoValue).IsRequired();
            var called = false;
            cmd.OnValidationError(_ =>
            {
                called = true;
            });
            app.Execute("sub");
            Assert.True(called, "Validation error should be called");
        }

        [Fact]
        public void ParentOptionsAreValidated()
        {
            var app = new CommandLineApplication();
            app.OnValidationError(_ => throw new InvalidOperationException());
            app.Option("-a --all", "all", CommandOptionType.NoValue).IsRequired();
            var called = false;
            var cmd = app.Command("sub", c => { });
            cmd.OnValidationError(_ =>
            {
                called = true;
            });
            var rc = app.Execute("sub");

            Assert.True(called, "Validation error should be called");
            Assert.NotEqual(0, rc);
        }

        [Subcommand(typeof(ValidationSubcommand))]
        private class ValidationParent
        {
            private int OnValidationError() => throw new InvalidOperationException();
        }

        [Command("sub")]
        private class ValidationSubcommand
        {
            [Option, Required]
            public bool All { get; }
            private int OnValidationError() => 10;
        }

        [Fact]
        public void ValidationErrorOnlyCalledOnSubcommand_AttributeBinding()
        {
            var rc = CommandLineApplication.Execute<ValidationParent>("sub");
            Assert.Equal(10, rc);
        }

        [Subcommand(typeof(EmptyValidationSubcommand))]
        private class ValidationParentWithRequiredOption
        {
            [Option, Required]
            public bool All { get; }
            private int OnValidationError() => throw new InvalidOperationException();
        }

        [Command("sub")]
        private class EmptyValidationSubcommand
        {
            private int OnValidationError() => 10;
        }

        [Fact]
        public void ParentOptionsAreValidated_AttributeBinding()
        {
            var rc = CommandLineApplication.Execute<ValidationParent>("sub");
            Assert.Equal(10, rc);
        }

        [Fact]
        public void DoesNotValidate_WhenShowingInfo()
        {
            var sb = new StringBuilder();
            var console = new TestConsole(_output)
            {
                Out = new StringWriter(sb),
            };
            var app = new CommandLineApplication(console);
            app.HelpOption();
            var errorMessage = "Version arg is required";
            app.Argument("version", "Arg").IsRequired(errorMessage: errorMessage);
            app.OnValidationError((_) => Assert.False(true, "Validation callbacks should not be executed"));

            Assert.Equal(0, app.Execute("--help"));
            Assert.DoesNotContain(errorMessage, sb.ToString());
        }

        [HelpOption]
        private class ProgramWithRequiredArg
        {
            [Argument(0), Required(ErrorMessage = "Arg is required")]
            public string? Version { get; }

            private void OnValidationError()
            {
                throw new InvalidOperationException("Validation callback should not be executed");
            }
        }

        [Fact]
        public void DoesNotValidate_WhenShowingInfo_AttributeBinding()
        {
            var sb = new StringBuilder();
            var console = new TestConsole(_output)
            {
                Out = new StringWriter(sb),
            };

            Assert.Equal(0, CommandLineApplication.Execute<ProgramWithRequiredArg>(console, "--help"));
            Assert.DoesNotContain("Arg is required", sb.ToString());
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        public void ValidatesRange(int input, bool isValid)
        {
            var app = new CommandLineApplication();
            app.Argument<int>("value", "").Accepts().Range(1, 2);
            var result = app.Parse(input.ToString());
            if (isValid)
            {
                Assert.Equal(ValidationResult.Success, result.SelectedCommand.GetValidationResult());
            }
            else
            {
                Assert.NotEqual(ValidationResult.Success, result.SelectedCommand.GetValidationResult());
            }
        }
    }
}
