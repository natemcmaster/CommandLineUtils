// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class ReflectionValidationErrorHandlerTests
    {
        private class DefaultErrorHandler
        {
            public bool WasCalled { get; private set; }

            public void OnValidationError()
            {
                WasCalled = true;
            }
        }

        private class ErrorHandlerWithCustomCode
        {
            public int OnValidationError()
            {
                return 42;
            }
        }

        private class ErrorHandlerWithValidationResult
        {
            public ValidationResult? ReceivedResult { get; private set; }

            public int OnValidationError(ValidationResult result)
            {
                ReceivedResult = result;
                return 5;
            }
        }

        private class ErrorHandlerThatThrows
        {
            public int OnValidationError()
            {
                throw new InvalidOperationException("Error handler failed");
            }
        }

        private class ErrorHandlerReturnsVoid
        {
            public bool WasCalled { get; private set; }

            public void OnValidationError(ValidationResult result)
            {
                WasCalled = true;
            }
        }

        [Fact]
        public void InvokesMethod_ReturnsDefaultErrorCode()
        {
            var model = new DefaultErrorHandler();
            var method = typeof(DefaultErrorHandler).GetMethod("OnValidationError")!;
            var handler = new ReflectionValidationErrorHandler(method);
            var validationResult = new ValidationResult("Error");

            var result = handler.Invoke(model, validationResult);

            Assert.True(model.WasCalled);
            Assert.Equal(1, result); // Default error code
        }

        [Fact]
        public void InvokesMethod_ReturnsCustomErrorCode()
        {
            var model = new ErrorHandlerWithCustomCode();
            var method = typeof(ErrorHandlerWithCustomCode).GetMethod("OnValidationError")!;
            var handler = new ReflectionValidationErrorHandler(method);
            var validationResult = new ValidationResult("Error");

            var result = handler.Invoke(model, validationResult);

            Assert.Equal(42, result);
        }

        [Fact]
        public void PassesValidationResult_ToMethod()
        {
            var model = new ErrorHandlerWithValidationResult();
            var method = typeof(ErrorHandlerWithValidationResult).GetMethod("OnValidationError")!;
            var handler = new ReflectionValidationErrorHandler(method);
            var validationResult = new ValidationResult("Specific error");

            handler.Invoke(model, validationResult);

            Assert.Same(validationResult, model.ReceivedResult);
        }

        [Fact]
        public void MethodThrows_PropagatesException()
        {
            var model = new ErrorHandlerThatThrows();
            var method = typeof(ErrorHandlerThatThrows).GetMethod("OnValidationError")!;
            var handler = new ReflectionValidationErrorHandler(method);
            var validationResult = new ValidationResult("Error");

            var ex = Assert.Throws<InvalidOperationException>(
                () => handler.Invoke(model, validationResult));

            Assert.Equal("Error handler failed", ex.Message);
        }

        [Fact]
        public void VoidReturnType_ReturnsDefaultErrorCode()
        {
            var model = new ErrorHandlerReturnsVoid();
            var method = typeof(ErrorHandlerReturnsVoid).GetMethod("OnValidationError")!;
            var handler = new ReflectionValidationErrorHandler(method);
            var validationResult = new ValidationResult("Error");

            var result = handler.Invoke(model, validationResult);

            Assert.True(model.WasCalled);
            Assert.Equal(1, result); // Default error code for void return
        }

        [Fact]
        public void ValidationResult_WithMemberNames_PassedCorrectly()
        {
            var model = new ErrorHandlerWithValidationResult();
            var method = typeof(ErrorHandlerWithValidationResult).GetMethod("OnValidationError")!;
            var handler = new ReflectionValidationErrorHandler(method);
            var validationResult = new ValidationResult("Field error", new[] { "Field1", "Field2" });

            handler.Invoke(model, validationResult);

            Assert.NotNull(model.ReceivedResult);
            Assert.Equal("Field error", model.ReceivedResult!.ErrorMessage);
            Assert.Contains("Field1", model.ReceivedResult.MemberNames);
            Assert.Contains("Field2", model.ReceivedResult.MemberNames);
        }
    }
}
