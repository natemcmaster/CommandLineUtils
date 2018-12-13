using System;
using System.IO;
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
        public void TestReturnCode()
        {
            Assert.Equal(42,
                new HostBuilder()
                    .ConfigureServices(collection => collection.AddSingleton<IConsole>(new TestConsole(_output)))
                    .RunCommandLineApplicationAsync<Return42Command>(new string[0])
                    .GetAwaiter()
                    .GetResult());
        }

        [Fact]
        public async void TestConsoleInjection()
        {
            var console = new Mock<IConsole>();
            var textWriter = new Mock<TextWriter>();
            textWriter.Setup(writer => writer.WriteLine("42")).Verifiable();
            console.SetupGet(c => c.Out).Returns(textWriter.Object);
            await new HostBuilder()
                .ConfigureServices(collection => collection.AddSingleton<IConsole>(console.Object))
                .RunCommandLineApplicationAsync<Write42Command>(new string[0]);
            Mock.Verify(console, textWriter);
        }

        [Fact]
        public async void TestConventionInjection()
        {
            var valueHolder = new ValueHolder<string[]>();
            var convention = new Mock<IConvention>();
            convention.Setup(c => c.Apply(It.IsAny<ConventionContext>()))
                .Callback((ConventionContext c) => c.Application.ThrowOnUnexpectedArgument = false).Verifiable();
            var args = new[] {"Capture", "some", "test", "arguments"};
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
        public void ItThrowsOnUnknownSubCommand()
        {
            var ex = Assert.Throws<UnrecognizedCommandParsingException>(
                () => new HostBuilder()
                    .ConfigureServices(collection => collection.AddSingleton<IConsole>(new TestConsole(_output)))
                    .RunCommandLineApplicationAsync<ParentCommand>(new string[] {"return41"})
                    .GetAwaiter()
                    .GetResult());
            Assert.Equal(new string[] {"return42"}, ex.NearestMatches);
        }

        [Fact]
        public void ItRethrowsThrownExceptions()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new HostBuilder()
                    .ConfigureServices(collection => collection.AddSingleton<IConsole>(new TestConsole(_output)))
                    .RunCommandLineApplicationAsync<ThrowsExceptionCommand>(new string[0])
                    .GetAwaiter()
                    .GetResult());
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
        {
            public T Value { get; set; }
        }

        [Command("Capture")]
        public class CaptureRemainingArgsCommand
        {
            public ValueHolder<string[]> ValueHolder { get; set; }

            public string[] RemainingArguments
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
