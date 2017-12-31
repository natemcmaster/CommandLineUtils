// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Internal;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CustomValidationAttributeTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("-c", "red")]
        [InlineData("-c", "blue")]
        public void CustomValidationAttributePasses(params string[] args)
        {
            var builder = new ReflectionAppBuilder<RedBlueProgram>();
            var result = builder.Bind(CreateContext(args ?? new string[0]));
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            var program = Assert.IsType<RedBlueProgram>(result.Target);
            if (args != null)
            {
                Assert.Equal(args[1], program.Color);
            }
        }

        [Theory]
        [InlineData("-c", "")]
        [InlineData("-c", null)]
        [InlineData("-c", "green")]
        public void CustomValidationAttributeFails(params string[] args)
        {
            var builder = new ReflectionAppBuilder<RedBlueProgram>();
            var result = builder.Bind(CreateContext(args));
            Assert.NotEqual(ValidationResult.Success, result.ValidationResult);
            var program = Assert.IsType<RedBlueProgram>(result.Target);
            if (args != null)
            {
                Assert.Equal(args[1], program.Color);
            }
            Assert.Equal("The value for --color must be 'red' or 'blue'", result.ValidationResult.ErrorMessage);
        }

        private class RedBlueProgram
        {
            [Option, RedOrBlue]
            public string Color { get; }
        }

        class RedOrBlueAttribute : ValidationAttribute
        {
            public RedOrBlueAttribute()
                : base("The value for {0} must be 'red' or 'blue'")
            {
            }

            protected override ValidationResult IsValid(object value, ValidationContext context)
            {
                if (value == null || (value is string str && str != "red" && str != "blue"))
                {
                    return new ValidationResult(FormatErrorMessage(context.DisplayName));
                }

                return ValidationResult.Success;
            }
        }

        private CommandLineContext CreateContext(string[] args)
            => new DefaultCommandLineContext(NullConsole.Singleton, Directory.GetCurrentDirectory(), args);
    }
}
