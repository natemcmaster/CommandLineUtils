// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ExecuteMethodConventionTests
    {
        private readonly ITestOutputHelper _output;

        public ExecuteMethodConventionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class ProgramWithExecute
        {
            private int OnExecute(CommandLineApplication<ProgramWithExecute> app)
            {
                return 42;
            }
        }

        [Fact]
        public void OnExecuteAddedViaConvention()
        {
            var app = new CommandLineApplication<ProgramWithExecute>();
            app.Conventions.UseOnExecuteMethodFromModel();
            var result = app.Execute();
            Assert.Equal(42, result);
        }

        private class ProgramWithBadOnExecute
        {
            internal long OnExecute()
            {
                return 0L;
            }
        }

        [Fact]
        public void ConventionThrowsIfOnExecuteDoesNotReturnIntOrVoid()
        {
            var app = new CommandLineApplication<ProgramWithBadOnExecute>();
            app.Conventions.UseOnExecuteMethodFromModel();
            var ex = Assert.Throws<InvalidOperationException>(() => app.Execute());
            Assert.Equal(Strings.InvalidOnExecuteReturnType(nameof(ProgramWithBadOnExecute.OnExecute)), ex.Message);
        }

        private class ProgramWithExecuteInjection
        {
            private int OnExecute(InvalidOperationException obj)
            {
                return 42;
            }
        }

        [Fact]
        public void OnExecuteGetsAdditionalServicesViaConvention()
        {
            var mock = new Mock<IServiceProvider>();
            mock.Setup(a => a.GetService(typeof(InvalidOperationException)))
                .Returns(new InvalidOperationException())
                .Verifiable();
            var app = new CommandLineApplication<ProgramWithExecuteInjection>();
            app.AdditionalServices = mock.Object;
            app.Conventions.UseOnExecuteMethodFromModel();
            var result = app.Execute();
            Assert.Equal(42, result);
            mock.Verify();
        }

        [Command]
        [Subcommand(typeof(SubcommandExecute))]
        private class MainExecute
        {
            private int OnExecute()
            {
                return 0;
            }
        }

        [Command("subcommand")]
        private class SubcommandExecute
        {
            private int OnExecute()
            {
                return 1;
            }
        }

        [Theory]
        [InlineData(null, 0)]
        [InlineData("subcommand", 1)]
        public void OnExecuteIsExecutedOnSelectedSubcommand(string? args, int expectedResult)
        {
            var app = new CommandLineApplication<MainExecute>();
            app.Conventions.UseSubcommandAttributes();
            app.Conventions.UseCommandAttribute();
            app.Conventions.UseOnExecuteMethodFromModel();

            var result = app.Execute(args?.Split(' ') ?? Util.EmptyArray<string>());
            Assert.Equal(expectedResult, result);
        }

        private class ProgramWithAsyncOnExecute
        {
            public CancellationToken Token { get; private set; }

            public Task<int> OnExecuteAsync(CancellationToken ct)
            {
                Token = ct;
                return Task.FromResult(4);
            }
        }

        [Fact]
        public async Task ItExecutesAsyncMethod()
        {
            var console = new TestConsole(_output);
            var app = new CommandLineApplication<ProgramWithAsyncOnExecute>(console);
            app.Conventions.UseOnExecuteMethodFromModel();

            var result = await app.ExecuteAsync(Array.Empty<string>());
            Assert.Equal(4, result);
            Assert.False(app.Model.Token.IsCancellationRequested);
            Assert.NotEqual(CancellationToken.None, app.Model.Token);
            console.CancelKeyCancellationSource.Cancel();
            Assert.True(app.Model.Token.IsCancellationRequested);
        }
    }
}
