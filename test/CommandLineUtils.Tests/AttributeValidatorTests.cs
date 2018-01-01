// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class AttributeValidatorTests
    {
        private readonly ITestOutputHelper _output;

        public AttributeValidatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ItOnlyInvokesAttributeIfValueExists()
        {
            var app = new CommandLineApplication();
            var arg = app.Argument("arg", "arg");
            var validator = new AttributeValidator(new ThrowingValidationAttribute());
            var factory = new CommandLineValidationContextFactory(app);
            var context = factory.Create(arg);

            Assert.Equal(ValidationResult.Success, validator.GetValidationResult(arg, context));

            arg.Values.Add(null);

            Assert.Throws<InvalidOperationException>(() => validator.GetValidationResult(arg, context));
        }

        [Theory]
        [InlineData(typeof(EmailAddressAttribute), "good@email.com", "bad")]
        [InlineData(typeof(PhoneAttribute), "(800) 555-5555", "xyz")]
        public void ItExecutesValidationAttribute(Type attributeType, string validValue, string invalidValue)
        {
            var attr = (ValidationAttribute)Activator.CreateInstance(attributeType);
            var app = new CommandLineApplication();
            var arg = app.Argument("arg", "arg");
            var validator = new AttributeValidator(attr);
            var factory = new CommandLineValidationContextFactory(app);
            var context = factory.Create(arg);

            arg.Values.Add(validValue);

            Assert.Equal(ValidationResult.Success, validator.GetValidationResult(arg, context));

            arg.Values.Clear();
            arg.Values.Add(invalidValue);
            var result = validator.GetValidationResult(arg, context);
            Assert.NotNull(result);
            Assert.NotEmpty(result.ErrorMessage);
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

        private class OptionBuilderApp : CommandLineApplication
        {
            public OptionBuilderApp(TestConsole testConsole)
                : base(testConsole)
            {
                Option("-e|--email", "Email", CommandOptionType.SingleValue)
                    .Accepts().EmailAddress();

                Option("-n|--name", "Name", CommandOptionType.SingleValue)
                    .Accepts().MinLength(1);

                Option("-a|--address", "Address", CommandOptionType.SingleValue)
                    .Accepts().MaxLength(10);

                Option("-r|--regex", "Regex", CommandOptionType.SingleValue)
                  .Accepts().RegularExpression("^abc.*");
            }
        }

        private class OptionApp
        {
            [Option, EmailAddress]
            public string Email { get; }

            [Option, MinLength(1)]
            public string Name { get; }

            [Option, MaxLength(10)]
            public string Address { get; }

            [Option, RegularExpression("^abc.*")]
            public string Regex { get; }

            private void OnExecute() { }
        }

        [Theory]
        [InlineData(new string[0], 0)]
        [InlineData(new[] { "-e", "" }, 1)]
        [InlineData(new[] { "-e", " " }, 1)]
        [InlineData(new[] { "-e", "email" }, 1)]
        [InlineData(new[] { "-e", "email@email@email" }, 1)]
        [InlineData(new[] { "-e", "email@example.com" }, 0)]
        [InlineData(new[] { "-n", "" }, 1)]
        [InlineData(new[] { "-n", "a" }, 0)]
        [InlineData(new[] { "-n", " " }, 0)]
        [InlineData(new[] { "-a", "abcdefghij" }, 0)]
        [InlineData(new[] { "-a", "abcdefghijk" }, 1)]
        [InlineData(new[] { "-r", "abcdefghijk" }, 0)]
        [InlineData(new[] { "-r", "xyz" }, 1)]
        public void ValidatesAttributesOnOption(string[] args, int exitCode)
        {
            Assert.Equal(exitCode, CommandLineApplication.Execute<OptionApp>(new TestConsole(_output), args));
            Assert.Equal(exitCode, new OptionBuilderApp(new TestConsole(_output)).Execute(args));
        }

        private sealed class ThrowingValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
