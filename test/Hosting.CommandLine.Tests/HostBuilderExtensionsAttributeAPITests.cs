// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using McMaster.Extensions.Hosting.CommandLine.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.Hosting.CommandLine.Tests
{
    public class HostBuilderExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public HostBuilderExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestReturnCode()
        {
            var exitCode = await new HostBuilder()
                    .ConfigureServices(collection => collection.AddSingleton<IConsole>(new TestConsole(_output)))
                    .RunCommandLineApplicationAsync<Return42Command>(Array.Empty<string>());
            Assert.Equal(42, exitCode);
        }

        [Fact]
        public async Task TestConsoleInjection()
        {
            var console = new Mock<IConsole>();
            var textWriter = new Mock<TextWriter>();
            textWriter.Setup(writer => writer.WriteLine("42")).Verifiable();
            console.SetupGet(c => c.Out).Returns(textWriter.Object);
            await new HostBuilder()
                .ConfigureServices(collection => collection.AddSingleton<IConsole>(console.Object))
                .RunCommandLineApplicationAsync<Write42Command>(Array.Empty<string>());
            Mock.Verify(console, textWriter);
        }

        [Fact]
        public async Task TestConventionInjection()
        {
            var valueHolder = new ValueHolder<string[]>();
            var convention = new Mock<IConvention>();
            convention.Setup(c => c.Apply(It.IsAny<ConventionContext>()))
                .Callback((ConventionContext c) =>
                    c.Application.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)
                .Verifiable();
            var args = new[] { "Capture", "some", "test", "arguments" };
            await new HostBuilder()
                .ConfigureServices(collection => collection
                    .AddSingleton<IConsole>(new TestConsole(_output))
                    .AddSingleton(valueHolder)
                    .AddSingleton(convention.Object))
                .RunCommandLineApplicationAsync<CaptureRemainingArgsCommand>(args);
            Assert.Equal(args, valueHolder.Value);
            Mock.Verify(convention);
        }

        [Fact]
        public async Task TestConfigureCommandLineApplication()
        {
            CommandLineApplication<Return42Command>? commandLineApp = null;
            await new HostBuilder()
                .RunCommandLineApplicationAsync<Return42Command>(
                Array.Empty<string>(),
                app => commandLineApp = app);
            Assert.NotNull(commandLineApp);
        }

        [Fact]
        public async Task TestUseCommandLineApplication()
        {
            CommandLineApplication<Return42Command>? commandLineApp = null;
            var hostBuilder = new HostBuilder();
            hostBuilder.UseCommandLineApplication<Return42Command>(Array.Empty<string>(), app => commandLineApp = app);
            var host = hostBuilder.Build();
            await host.RunCommandLineApplicationAsync();
            Assert.NotNull(commandLineApp);
        }

        [Fact]
        public async Task UseCommandLineApplicationReThrowsExceptions()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => new HostBuilder()
                    .ConfigureServices(collection => collection.AddSingleton<IConsole>(new TestConsole(_output)))
                    .UseCommandLineApplication<ThrowsExceptionCommand>(Array.Empty<string>())
                    .Build()
                    .RunCommandLineApplicationAsync());
            Assert.Equal("A test", ex.Message);
        }

        [Fact]
        public async Task ItThrowsOnUnknownSubCommand()
        {
            var ex = await Assert.ThrowsAsync<UnrecognizedCommandParsingException>(
                () => new HostBuilder()
                    .ConfigureServices(collection => collection.AddSingleton<IConsole>(new TestConsole(_output)))
                    .RunCommandLineApplicationAsync<ParentCommand>(new string[] { "return41" }));
            Assert.Equal(new string[] { "return42" }, ex.NearestMatches);
        }

        [Fact]
        public async Task ItRethrowsThrownExceptions()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => new HostBuilder()
                    .ConfigureServices(collection => collection.AddSingleton<IConsole>(new TestConsole(_output)))
                    .RunCommandLineApplicationAsync<ThrowsExceptionCommand>(Array.Empty<string>()));
            Assert.Equal("A test", ex.Message);
        }

        [Command]
        public class Return42Command
        {
            private int OnExecute()
            {
                return 42;
            }
        }

        [Command]
        public class Write42Command
        {
            private void OnExecute(CommandLineApplication<Write42Command> app)
            {
                app.Out.WriteLine("42");
            }
        }

        public class ValueHolder<T>
            where T : class
        {
            public T? Value { get; set; }
        }

        [Command("Capture")]
        public class CaptureRemainingArgsCommand
        {
            public ValueHolder<string[]> ValueHolder { get; set; }

            public string[]? RemainingArguments
            {
                get => ValueHolder.Value;
                set => ValueHolder.Value = value;
            }

            public CaptureRemainingArgsCommand(ValueHolder<string[]> valueHolder)
            {
                ValueHolder = valueHolder;
            }

            private void OnExecute(CommandLineApplication<CaptureRemainingArgsCommand> app)
            {
            }
        }

        [Command]
        [Subcommand(typeof(Return42Command))]
        class ParentCommand
        {
        }

        [Command]
        class ThrowsExceptionCommand
        {
            private int OnExecute()
            {
                throw new InvalidOperationException("A test");
            }
        }
    }
}
