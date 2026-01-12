// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.Conventions
{
    public class CommandAttributeConventionTests
    {
        #region ValidationAttribute on Model Type Tests

        /// <summary>
        /// Custom validation attribute for testing.
        /// </summary>
        private class CustomModelValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object? value)
            {
                return true;
            }
        }

        [CustomModelValidation]
        private class ModelWithValidationAttribute
        {
            [Option("-n|--name")]
            public string? Name { get; set; }
        }

        [Fact]
        public void Apply_AddsValidationAttributeFromModelType()
        {
            // This tests lines 50-53: processing ValidationAttribute on model type
            var app = new CommandLineApplication<ModelWithValidationAttribute>();
            app.Conventions.AddConvention(new CommandAttributeConvention());

            // Verify that a validator was added
            Assert.NotEmpty(app.Validators);
            Assert.Single(app.Validators);
        }

        private class SecondCustomValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object? value) => true;
        }

        [CustomModelValidation]
        [SecondCustomValidation]
        private class ModelWithMultipleValidationAttributes
        {
            [Option]
            public string? Value { get; set; }
        }

        [Fact]
        public void Apply_AddsMultipleValidationAttributesFromModelType()
        {
            var app = new CommandLineApplication<ModelWithMultipleValidationAttributes>();
            app.Conventions.AddConvention(new CommandAttributeConvention());

            // Should have two validators from the two attributes
            Assert.Equal(2, app.Validators.Count);
        }

        #endregion

        #region Fallback Path Tests

        private class ModelWithoutCommandAttribute
        {
            [Option("-v|--verbose")]
            public bool Verbose { get; set; }
        }

        [Fact]
        public void Apply_HandlesModelWithoutCommandAttribute()
        {
            // Tests line 36: attribute?.Configure() when attribute is null
            var app = new CommandLineApplication<ModelWithoutCommandAttribute>();
            app.Name = "test-app";
            app.Conventions.AddConvention(new CommandAttributeConvention());

            // Name should remain unchanged since no CommandAttribute
            Assert.Equal("test-app", app.Name);
        }

        [Command("my-cmd", Description = "Test command")]
        private class ModelWithCommandAttribute
        {
            [Option]
            public string? Option1 { get; set; }
        }

        [Fact]
        public void Apply_ProcessesCommandAttribute()
        {
            var app = new CommandLineApplication<ModelWithCommandAttribute>();
            app.Conventions.AddConvention(new CommandAttributeConvention());

            Assert.Equal("my-cmd", app.Name);
            Assert.Equal("Test command", app.Description);
        }

        #endregion

        #region Subcommand Processing Tests

        [Command("parent")]
        [Subcommand(typeof(ChildCommand))]
        private class ParentCommand
        {
        }

        [Command("child", Description = "Child command")]
        private class ChildCommand
        {
        }

        [Fact]
        public void Apply_ProcessesSubcommands()
        {
            // Tests lines 39-45: recursive processing of subcommands
            var app = new CommandLineApplication<ParentCommand>();
            app.Conventions.UseDefaultConventions();

            Assert.Equal("parent", app.Name);
            var subCmd = app.Commands.FirstOrDefault();
            Assert.NotNull(subCmd);
            Assert.Equal("child", subCmd.Name);
            Assert.Equal("Child command", subCmd.Description);
        }

        #endregion

        #region Non-Generic Application Tests

        [Fact]
        public void Apply_ReturnsEarly_WhenModelTypeIsNull()
        {
            // Tests lines 20-22: early return when ModelType is null
            var app = new CommandLineApplication();
            var convention = new CommandAttributeConvention();

            // Should not throw, just return early
            convention.Apply(new ConventionContext(app, null));

            // App should be unchanged
            Assert.Null(app.Description);
        }

        #endregion
    }
}
