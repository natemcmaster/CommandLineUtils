// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.Internal.Validators;
using McMaster.Extensions.CommandLineUtils.Validation;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ValidatorChainTests
    {
        private readonly ValidatorChainBuilder _is = new ValidatorChainBuilder();

        [Fact]
        public void BooleanOr()
        {
            // tests that .Or short circuit's if the first condition is true
            Assert.True(GetResult(
                _is.True().Or.Throw()));

            Assert.True(GetResult(
               _is.False().Or.True()));

            Assert.True(GetResult(
               _is.False().Or.False().Or.True()));

            Assert.True(GetResult(
               _is.True().Or.True()));

            Assert.True(GetResult(
                _is.True().Or.False()));

            Assert.False(GetResult(
                _is.False().Or.False()));

            Assert.Throws<InvalidOperationException>(
                () => GetResult(_is.False().Or.Throw()));
        }

        [Fact]
        public void BooleanAnd()
        {
            Assert.True(GetResult(
                _is.True().And.True()));

            Assert.False(GetResult(
                _is.False().And.True()));

            Assert.False(GetResult(
                _is.False().And.Throw()));

            Assert.False(GetResult(
                _is.False().And.False().And.True()));

            Assert.False(GetResult(
                _is.True().And.True().And.False()));

            Assert.True(GetResult(
                _is.True().And.True()));

            Assert.False(GetResult(
                _is.False().And.False()));

            Assert.Throws<InvalidOperationException>(
                () => GetResult(_is.True().And.Throw()));
        }

        private bool GetResult(IValidator rule)
            => ValidationResult.Success == rule.GetValidationResult(new CommandArgument(), new ValidationContext(new object()));
    }
}
