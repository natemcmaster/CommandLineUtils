// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
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
            public string Param { get; set; }

            private void OnExecute() { }
        }

        [Fact]
        public void RequiredOption_Attribute_Pass()
        {
            Assert.Equal(0, CommandLineApplication.Execute<RequiredOption>("-p", "p"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new object[] { new[] { "-p", "" } })]
        [InlineData(new object[] { new[] { "-p", " " } })]
        public void RequiredOption_Attribute_Fail(string[] args)
        {
            Assert.NotEqual(0, CommandLineApplication.Execute<RequiredOption>(new TestConsole(_output), args));
        }

        [Theory]
        [InlineData(new[] { "" }, false)]
        [InlineData(new string[0], false)]
        [InlineData(new[] { " " }, false)]
        [InlineData(new[] { "val", "" }, false)]
        [InlineData(new string[] { null }, false)]
        public void RequiredArgument_Fail(string[] args, bool allowEmptyStrings)
        {
            var app = new CommandLineApplication(new TestConsole(_output));
            app.Argument("Test", "Test arg", multipleValues: true).IsRequired(allowEmptyStrings);
            Assert.NotEqual(0, app.Execute(args));
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
            Assert.Equal(expected, validation.ErrorMessage);
        }

        [Fact]
        public void RequiredArgument_ErrorMessage()
        {
            var required = new RequiredAttribute();
            var expected = required.FormatErrorMessage("Arg");
            var app = new CommandLineApplication();
            app.Argument("Arg", string.Empty).IsRequired();
            var validation = app.GetValidationResult();
            Assert.Equal(expected, validation.ErrorMessage);
        }

        private class EmailArgumentApp
        {
            [Argument(0), EmailAddress]
            public string Email { get; }
            private void OnExecute() { }
        }

        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 1)]
        [InlineData(" ", 1)]
        [InlineData("email", 1)]
        [InlineData("email@email@email", 1)]
        [InlineData("email@example.com", 0)]
        public void ValidatesEmailArgument(string email, int exitCode)
        {
            Assert.Equal(exitCode, CommandLineApplication.Execute<EmailArgumentApp>(email));
        }

        private class EmailOptionApp
        {
            [Option, EmailAddress]
            public string Email { get; }
            private void OnExecute() { }
        }

        [Theory]
        [InlineData(new string[0], 0)]
        [InlineData(new[] { "-e", "" }, 1)]
        [InlineData(new[] { "-e", " " }, 1)]
        [InlineData(new[] { "-e", "email" }, 1)]
        [InlineData(new[] { "-e", "email@email@email" }, 1)]
        [InlineData(new[] { "-e", "email@example.com" }, 0)]
        public void ValidatesEmailOption(string[] args, int exitCode)
        {
            Assert.Equal(exitCode, CommandLineApplication.Execute<EmailOptionApp>(args));
        }
    }
}
