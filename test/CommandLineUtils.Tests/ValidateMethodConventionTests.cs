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
    }
}
