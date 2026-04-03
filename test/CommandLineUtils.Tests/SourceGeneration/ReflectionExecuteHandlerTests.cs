// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class ReflectionExecuteHandlerTests
    {
        private class SyncVoidCommand
        {
            public bool WasExecuted { get; private set; }

            public void OnExecute()
            {
                WasExecuted = true;
            }
        }

        private class SyncIntCommand
        {
            public int ReturnCode { get; set; } = 42;

            public int OnExecute()
            {
                return ReturnCode;
            }
        }

        private class AsyncVoidCommand
        {
            public bool WasExecuted { get; private set; }

            public async Task OnExecuteAsync()
            {
                await Task.Delay(1);
                WasExecuted = true;
            }
        }

        private class AsyncIntCommand
        {
            public int ReturnCode { get; set; } = 99;

            public async Task<int> OnExecuteAsync()
            {
                await Task.Delay(1);
                return ReturnCode;
            }
        }

        private class CommandWithAppParameter
        {
            public CommandLineApplication? ReceivedApp { get; private set; }

            public void OnExecute(CommandLineApplication app)
            {
                ReceivedApp = app;
            }
        }

        private class CommandWithCancellationToken
        {
            public CancellationToken ReceivedToken { get; private set; }

            public void OnExecute(CancellationToken token)
            {
                ReceivedToken = token;
            }
        }

        private class CommandThatThrows
        {
            public void OnExecute()
            {
                throw new InvalidOperationException("Test exception");
            }
        }

        private class AsyncCommandThatThrows
        {
            public async Task<int> OnExecuteAsync()
            {
                await Task.Delay(1);
                throw new InvalidOperationException("Async test exception");
            }
        }

        [Fact]
        public async Task SyncVoidMethod_ExecutesAndReturnsZero()
        {
            var model = new SyncVoidCommand();
            var method = typeof(SyncVoidCommand).GetMethod("OnExecute")!;
            var handler = new ReflectionExecuteHandler(method, isAsync: false);
            var app = new CommandLineApplication();

            var result = await handler.InvokeAsync(model, app, CancellationToken.None);

            Assert.True(model.WasExecuted);
            Assert.Equal(0, result);
            Assert.False(handler.IsAsync);
        }

        [Fact]
        public async Task SyncIntMethod_ReturnsCorrectValue()
        {
            var model = new SyncIntCommand { ReturnCode = 42 };
            var method = typeof(SyncIntCommand).GetMethod("OnExecute")!;
            var handler = new ReflectionExecuteHandler(method, isAsync: false);
            var app = new CommandLineApplication();

            var result = await handler.InvokeAsync(model, app, CancellationToken.None);

            Assert.Equal(42, result);
        }

        [Fact]
        public async Task AsyncVoidMethod_ExecutesAndReturnsZero()
        {
            var model = new AsyncVoidCommand();
            var method = typeof(AsyncVoidCommand).GetMethod("OnExecuteAsync")!;
            var handler = new ReflectionExecuteHandler(method, isAsync: true);
            var app = new CommandLineApplication();

            var result = await handler.InvokeAsync(model, app, CancellationToken.None);

            Assert.True(model.WasExecuted);
            Assert.Equal(0, result);
            Assert.True(handler.IsAsync);
        }

        [Fact]
        public async Task AsyncIntMethod_ReturnsCorrectValue()
        {
            var model = new AsyncIntCommand { ReturnCode = 99 };
            var method = typeof(AsyncIntCommand).GetMethod("OnExecuteAsync")!;
            var handler = new ReflectionExecuteHandler(method, isAsync: true);
            var app = new CommandLineApplication();

            var result = await handler.InvokeAsync(model, app, CancellationToken.None);

            Assert.Equal(99, result);
        }

        [Fact]
        public async Task PassesCommandLineApplication_ToMethod()
        {
            var model = new CommandWithAppParameter();
            var method = typeof(CommandWithAppParameter).GetMethod("OnExecute")!;
            var handler = new ReflectionExecuteHandler(method, isAsync: false);
            var app = new CommandLineApplication();

            await handler.InvokeAsync(model, app, CancellationToken.None);

            Assert.Same(app, model.ReceivedApp);
        }

        [Fact]
        public async Task PassesCancellationToken_ToMethod()
        {
            var model = new CommandWithCancellationToken();
            var method = typeof(CommandWithCancellationToken).GetMethod("OnExecute")!;
            var handler = new ReflectionExecuteHandler(method, isAsync: false);
            var app = new CommandLineApplication();
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            await handler.InvokeAsync(model, app, token);

            Assert.Equal(token, model.ReceivedToken);
        }

        [Fact]
        public async Task SyncMethod_ThrowsException_Propagates()
        {
            var model = new CommandThatThrows();
            var method = typeof(CommandThatThrows).GetMethod("OnExecute")!;
            var handler = new ReflectionExecuteHandler(method, isAsync: false);
            var app = new CommandLineApplication();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.InvokeAsync(model, app, CancellationToken.None));

            Assert.Equal("Test exception", ex.Message);
        }

        [Fact]
        public async Task AsyncMethod_ThrowsException_Propagates()
        {
            var model = new AsyncCommandThatThrows();
            var method = typeof(AsyncCommandThatThrows).GetMethod("OnExecuteAsync")!;
            var handler = new ReflectionExecuteHandler(method, isAsync: true);
            var app = new CommandLineApplication();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.InvokeAsync(model, app, CancellationToken.None));

            Assert.Equal("Async test exception", ex.Message);
        }

        [Fact]
        public void IsAsync_ReflectsConstructorParameter()
        {
            var syncMethod = typeof(SyncVoidCommand).GetMethod("OnExecute")!;
            var asyncMethod = typeof(AsyncVoidCommand).GetMethod("OnExecuteAsync")!;

            var syncHandler = new ReflectionExecuteHandler(syncMethod, isAsync: false);
            var asyncHandler = new ReflectionExecuteHandler(asyncMethod, isAsync: true);

            Assert.False(syncHandler.IsAsync);
            Assert.True(asyncHandler.IsAsync);
        }
    }
}
