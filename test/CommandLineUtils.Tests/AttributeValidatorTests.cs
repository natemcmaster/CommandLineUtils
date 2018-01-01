// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class AttributeValidatorTests
    {
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

        private sealed class ThrowingValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
