// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Internal;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class ReflectionValidateHandlerTests
    {
        private class ValidCommand
        {
            public bool WasValidated { get; private set; }

            public ValidationResult? OnValidate()
            {
                WasValidated = true;
                return ValidationResult.Success;
            }
        }

        private class InvalidCommand
        {
            public ValidationResult? OnValidate()
            {
                return new ValidationResult("Validation failed");
            }
        }

        private class CommandWithValidationContext
        {
            public ValidationContext? ReceivedContext { get; private set; }

            public ValidationResult? OnValidate(ValidationContext context)
            {
                ReceivedContext = context;
                return ValidationResult.Success;
            }
        }

        private class CommandWithCommandLineContext
        {
            public CommandLineContext? ReceivedContext { get; private set; }

            public ValidationResult? OnValidate(CommandLineContext context)
            {
                ReceivedContext = context;
                return ValidationResult.Success;
            }
        }

        private class CommandWithBothContexts
        {
            public ValidationContext? ReceivedValidationContext { get; private set; }
            public CommandLineContext? ReceivedCommandContext { get; private set; }

            public ValidationResult? OnValidate(ValidationContext validationContext, CommandLineContext commandContext)
            {
                ReceivedValidationContext = validationContext;
                ReceivedCommandContext = commandContext;
                return ValidationResult.Success;
            }
        }

        private class CommandThatThrows
        {
            public ValidationResult? OnValidate()
            {
                throw new InvalidOperationException("Validation error");
            }
        }

        [Fact]
        public void InvokesMethod_ReturnsSuccess()
        {
            var model = new ValidCommand();
            var method = typeof(ValidCommand).GetMethod("OnValidate")!;
            var handler = new ReflectionValidateHandler(method);
            var validationContext = new ValidationContext(model);
            var commandContext = CreateCommandLineContext();

            var result = handler.Invoke(model, validationContext, commandContext);

            Assert.True(model.WasValidated);
            Assert.Same(ValidationResult.Success, result);
        }

        [Fact]
        public void InvokesMethod_ReturnsFailure()
        {
            var model = new InvalidCommand();
            var method = typeof(InvalidCommand).GetMethod("OnValidate")!;
            var handler = new ReflectionValidateHandler(method);
            var validationContext = new ValidationContext(model);
            var commandContext = CreateCommandLineContext();

            var result = handler.Invoke(model, validationContext, commandContext);

            Assert.NotNull(result);
            Assert.Equal("Validation failed", result!.ErrorMessage);
        }

        [Fact]
        public void PassesValidationContext_ToMethod()
        {
            var model = new CommandWithValidationContext();
            var method = typeof(CommandWithValidationContext).GetMethod("OnValidate")!;
            var handler = new ReflectionValidateHandler(method);
            var validationContext = new ValidationContext(model);
            var commandContext = CreateCommandLineContext();

            handler.Invoke(model, validationContext, commandContext);

            Assert.Same(validationContext, model.ReceivedContext);
        }

        [Fact]
        public void PassesCommandLineContext_ToMethod()
        {
            var model = new CommandWithCommandLineContext();
            var method = typeof(CommandWithCommandLineContext).GetMethod("OnValidate")!;
            var handler = new ReflectionValidateHandler(method);
            var validationContext = new ValidationContext(model);
            var commandContext = CreateCommandLineContext();

            handler.Invoke(model, validationContext, commandContext);

            Assert.Same(commandContext, model.ReceivedContext);
        }

        [Fact]
        public void PassesBothContexts_ToMethod()
        {
            var model = new CommandWithBothContexts();
            var method = typeof(CommandWithBothContexts).GetMethod("OnValidate")!;
            var handler = new ReflectionValidateHandler(method);
            var validationContext = new ValidationContext(model);
            var commandContext = CreateCommandLineContext();

            handler.Invoke(model, validationContext, commandContext);

            Assert.Same(validationContext, model.ReceivedValidationContext);
            Assert.Same(commandContext, model.ReceivedCommandContext);
        }

        [Fact]
        public void MethodThrows_PropagatesException()
        {
            var model = new CommandThatThrows();
            var method = typeof(CommandThatThrows).GetMethod("OnValidate")!;
            var handler = new ReflectionValidateHandler(method);
            var validationContext = new ValidationContext(model);
            var commandContext = CreateCommandLineContext();

            var ex = Assert.Throws<InvalidOperationException>(
                () => handler.Invoke(model, validationContext, commandContext));

            Assert.Equal("Validation error", ex.Message);
        }

        private static CommandLineContext CreateCommandLineContext()
        {
            var app = new CommandLineApplication();
            return new DefaultCommandLineContext(
                PhysicalConsole.Singleton,
                Directory.GetCurrentDirectory(),
                Array.Empty<string>());
        }
    }
}
