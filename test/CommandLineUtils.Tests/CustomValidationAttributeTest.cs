// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
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
            var app = new CommandLineApplication<RedBlueProgram>();
            app.Conventions.UseDefaultConventions();
            var result = app.Parse(args ?? System.Array.Empty<string>());
            Assert.Equal(ValidationResult.Success, result.SelectedCommand.GetValidationResult());
            var program = Assert.IsType<CommandLineApplication<RedBlueProgram>>(result.SelectedCommand);
            Assert.Same(app, program);
            if (args != null)
            {
                Assert.Equal(args[1], app.Model.Color);
            }
        }

        [Theory]
        [InlineData("-c", "")]
        [InlineData("-c", null)]
        [InlineData("-c", "green")]
        public void CustomValidationAttributeFails(params string[] args)
        {
            var app = new CommandLineApplication<RedBlueProgram>();
            app.Conventions.UseAttributes();
            var result = app.Parse(args);
            var validationResult = result.SelectedCommand.GetValidationResult();
            Assert.NotEqual(ValidationResult.Success, validationResult);
            var program = Assert.IsType<CommandLineApplication<RedBlueProgram>>(result.SelectedCommand);
            Assert.Same(app, program);
            if (args != null)
            {
                Assert.Equal(args[1], app.Model.Color);
            }
            Assert.Equal("The value for --color must be 'red' or 'blue'", validationResult.ErrorMessage);
        }

        private class RedBlueProgram
        {
            [Option, RedOrBlue]
            public string? Color { get; }
        }

        class RedOrBlueAttribute : ValidationAttribute
        {
            public RedOrBlueAttribute()
                : base("The value for {0} must be 'red' or 'blue'")
            {
            }

            protected override ValidationResult? IsValid(object? value, ValidationContext context)
            {
                if (value == null || (value is string str && str != "red" && str != "blue"))
                {
                    return new ValidationResult(FormatErrorMessage(context.DisplayName));
                }

                return ValidationResult.Success;
            }
        }
    }
}
