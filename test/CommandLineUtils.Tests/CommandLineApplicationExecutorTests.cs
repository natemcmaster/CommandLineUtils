// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandLineApplicationExecutorTests
    {
        private readonly ITestOutputHelper _output;

        public CommandLineApplicationExecutorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class VoidExecuteMethodWithNoArgs
        {
            [Option]
            public string Message { get; set; }

            private void OnExecute()
            {
                Assert.Equal("Add attribute parsing", Message);
            }
        }

        [Fact]
        public void ExecutesVoidMethod()
        {
            var rc = CommandLineApplication.Execute<VoidExecuteMethodWithNoArgs>("-m", "Add attribute parsing");

            Assert.Equal(0, rc);
        }

        private class IntExecuteMethodWithNoArgs
        {
            [Option]
            public int Count { get; set; }

            private int OnExecute()
            {
                return Count;
            }
        }

        [Fact]
        public void ExecutesIntMethod()
        {
            var rc = CommandLineApplication.Execute<IntExecuteMethodWithNoArgs>("-c", "5");

            Assert.Equal(5, rc);
        }

        private class OverloadExecute
        {
            private int OnExecute() => 0;

            private void OnExecute(string a)
            { }
        }

        [Fact]
        public void ThrowsIfOverloaded()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CommandLineApplication.Execute<OverloadExecute>());

            Assert.Equal(Strings.AmbiguousOnExecuteMethod, ex.Message);
        }

        private class AmbiguousExecute
        {
            private int OnExecute() => 0;

            private Task OnExecuteAsync() => Task.FromResult(0);
        }

        [Fact]
        public void ThrowsIfMethodIsAmbiguous()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CommandLineApplication.Execute<AmbiguousExecute>());

            Assert.Equal(Strings.AmbiguousOnExecuteMethod, ex.Message);
        }

        private class NoExecuteMethod
        { }

        [Fact]
        public void ThrowsIfNoMethod()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CommandLineApplication.Execute<NoExecuteMethod>());

            Assert.Equal(Strings.NoOnExecuteMethodFound, ex.Message);
        }

        private class BadReturnType
        {
            private string OnExecute() => null;
        }

        [Fact]
        public void ThrowsIfMethodDoesNotReturnVoidOrInt()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CommandLineApplication.Execute<BadReturnType>());

            Assert.Equal(Strings.InvalidOnExecuteReturnType("OnExecute"), ex.Message);
        }

        private class BadAsyncReturnType
        {
            private Task<string> OnExecute() => null;
        }

        [Fact]
        public async Task ThrowsIfAsyncMethodDoesNotReturnVoidOrInt()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await CommandLineApplication.ExecuteAsync<BadReturnType>());

            Assert.Equal(Strings.InvalidOnExecuteReturnType("OnExecute"), ex.Message);
        }

        private class ExecuteWithTypes
        {
            private void OnExecute(CommandLineApplication application, IConsole console)
            {
                Assert.NotNull(application);
                Assert.NotNull(console);
            }
        }

        [Fact]
        public void PassesInKnownParameterTypes()
        {
            Assert.Equal(0, CommandLineApplication.Execute<ExecuteWithTypes>(new string[0]));
        }

        private class ExecuteWithUnknownTypes
        {
            private void OnExecute(string other)
            {
            }
        }

        [Fact]
        public void ThrowsForUnknownOnExecuteTypes()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CommandLineApplication.Execute<ExecuteWithUnknownTypes>());
            var method = typeof(ExecuteWithUnknownTypes).GetTypeInfo().GetMethod("OnExecute", BindingFlags.Instance | BindingFlags.NonPublic);
            var param = Assert.Single(method.GetParameters());
            Assert.Equal(Strings.UnsupportedParameterTypeOnMethod(method.Name, param), ex.Message);
        }

        private class ExecuteAsyncWithInt
        {
            private async Task<int> OnExecute()
            {
                await Task.CompletedTask;
                return 1;
            }
        }

        [Fact]
        public async Task ExecutesAsyncWithInt()
        {
            Assert.Equal(1, await CommandLineApplication.ExecuteAsync<ExecuteAsyncWithInt>(new string[0]));
        }

        private class ExecuteAsync
        {
            private async Task OnExecuteAsync()
            {
                await Task.CompletedTask;
            }
        }

        [Fact]
        public async Task ExecutesAsync()
        {
            Assert.Equal(0, await CommandLineApplication.ExecuteAsync<ExecuteAsync>(new string[0]));
        }

        [HelpOption]
        [VersionOption("1.2.3")]
        private class HelpClass
        {
            private void OnExecute()
            {
                Assert.True(false, "This should not execute");
            }
        }

        [Theory]
        [InlineData("--help")]
        [InlineData("--version")]
        public void DoesNotInvokeOnExecuteWhenShowingInfo(string arg)
        {
            var rc = CommandLineApplication.Execute<HelpClass>(new TestConsole(_output), arg);
            Assert.Equal(0, rc);
        }

        private class SyncOnExecute
        {
            private int OnExecute() => 8;
        }

        [Fact]
        public async Task FallsBackToSynchronous()
        {
            Assert.Equal(8, await CommandLineApplication.ExecuteAsync<SyncOnExecute>());
        }

        private class TaskReturnType
        {
            private Task OnExecute() => Task.FromResult(200);
        }

        [Fact]
        public async Task ReturnsIntEvenIfReturnTypeIsNotGeneric()
        {
            Assert.Equal(200, await CommandLineApplication.ExecuteAsync<TaskReturnType>());
        }
    }
}
