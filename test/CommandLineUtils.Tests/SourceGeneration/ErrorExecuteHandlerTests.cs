// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class ErrorExecuteHandlerTests
    {
        [Fact]
        public void Constructor_WithNullErrorMessage_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ErrorExecuteHandler(null!));

            Assert.Equal("errorMessage", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithValidErrorMessage_DoesNotThrow()
        {
            var handler = new ErrorExecuteHandler("Test error message");

            Assert.NotNull(handler);
        }

        [Fact]
        public void IsAsync_ReturnsFalse()
        {
            var handler = new ErrorExecuteHandler("Test error message");

            Assert.False(handler.IsAsync);
        }

        [Fact]
        public async Task InvokeAsync_ThrowsInvalidOperationException_WithErrorMessage()
        {
            var errorMessage = "Ambiguous OnExecute methods detected";
            var handler = new ErrorExecuteHandler(errorMessage);
            var app = new CommandLineApplication();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.InvokeAsync(new object(), app, CancellationToken.None));

            Assert.Equal(errorMessage, ex.Message);
        }
    }
}
