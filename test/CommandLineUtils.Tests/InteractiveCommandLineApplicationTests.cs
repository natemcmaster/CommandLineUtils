// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils.IO;
using Moq;
using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class InteractiveCommandLineApplicationTests
    {
        private readonly ITestOutputHelper _output;

        public InteractiveCommandLineApplicationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ShowWelcomeMessageViaWelcomeAction()
        {
            var promptMock = new Mock<IPrompt>();
            var sb = new StringBuilder();
            var output = new StringWriter(sb);
            var console = new TestConsole(_output)
            {
                Out = output,
            };

            var welcomeMessage = "Interactive CommandLine Application Test";
            var app = new CommandLineApplication(
                promptMock.Object,
                new InteractiveExecuteSettings() { WelcomeAction = (c) => c.WriteLine(welcomeMessage) },
                console);

            promptMock
                .Setup(prompt => prompt.GetString(It.IsAny<string>(), null, It.IsAny<ConsoleColor?>(), It.IsAny<ConsoleColor?>()))
                .Returns("quit");

            var result = app.ExecuteInteractive();

            Assert.Equal(0, result);
            Assert.Contains(welcomeMessage, sb.ToString());
        }

        [Fact]
        public void ShowWelcomeMessageViaWelcomeMessage()
        {
            var promptMock = new Mock<IPrompt>();
            var sb = new StringBuilder();
            var output = new StringWriter(sb);
            var console = new TestConsole(_output)
            {
                Out = output,
            };

            var welcomeMessage = "Interactive CommandLine Application Test";
            var app = new CommandLineApplication(
                promptMock.Object,
                new InteractiveExecuteSettings() { WelcomeMessage = welcomeMessage },
                console);

            promptMock
                .Setup(prompt => prompt.GetString(It.IsAny<string>(), null, It.IsAny<ConsoleColor?>(), It.IsAny<ConsoleColor?>()))
                .Returns("quit");

            var result = app.ExecuteInteractive();

            Assert.Equal(0, result);
            Assert.Contains(welcomeMessage, sb.ToString());
        }

        [Fact]
        public void StopInteractiveSessionWithDefaultQuitCommand()
        {
            var promptMock = new Mock<IPrompt>();
            var app = new CommandLineApplication(promptMock.Object);

            promptMock
                .Setup(prompt => prompt.GetString(It.IsAny<string>(), null, It.IsAny<ConsoleColor?>(), It.IsAny<ConsoleColor?>()))
                .Returns("quit");

            var result = app.ExecuteInteractive();

            Assert.Equal(0, result);
            Assert.False(app.IsInteractive);
        }

        [Fact]
        public void StopInteractiveSessionWithCustomQuitCommand()
        {
            var promptMock = new Mock<IPrompt>();
            var app = new CommandLineApplication(promptMock.Object, new InteractiveExecuteSettings { UseDefaultQuitCommand = false });

            app.Command("quiter", c =>
            {
                c.OnExecute(() =>
                {
                    app.IsInteractive = false;
                    return 5;
                });
            });

            promptMock
                .Setup(prompt => prompt.GetString(It.IsAny<string>(), null, It.IsAny<ConsoleColor?>(), It.IsAny<ConsoleColor?>()))
                .Returns("quiter");

            var result = app.ExecuteInteractive();

            Assert.Equal(5, result);
            Assert.False(app.IsInteractive);
        }

        [Fact]
        public void ContinueAfterException()
        {
            var promptMock = new Mock<IPrompt>();
            var sb = new StringBuilder();
            var output = new StringWriter(sb);
            var console = new TestConsole(_output)
            {
                Error = output,
            };

            var app = new CommandLineApplication(promptMock.Object, console: console);

            app.Command("quit", c =>
            {
                c.OnExecute(() =>
                {
                    app.IsInteractive = false;
                    return 5;
                });
            });

            var nonExistingCommandName = "NonExistingCommand";
            promptMock
                .SetupSequence(prompt => prompt.GetString(It.IsAny<string>(), null, It.IsAny<ConsoleColor?>(), It.IsAny<ConsoleColor?>()))
                .Returns(nonExistingCommandName)
                .Returns("quit");

            var result = app.ExecuteInteractive();

            Assert.Contains($"Unrecognized command or argument '{nonExistingCommandName}'", sb.ToString());
            Assert.Equal(5, result);
            Assert.False(app.IsInteractive);
        }

        [Fact]
        public void EndAfterException()
        {
            var promptMock = new Mock<IPrompt>();
            var sb = new StringBuilder();
            var output = new StringWriter(sb);
            var console = new TestConsole(_output)
            {
                Error = output,
            };

            var app = new CommandLineApplication(promptMock.Object, new InteractiveExecuteSettings() { EndSessionOnException = true }, console);

            app.Command("quit", c =>
            {
                c.OnExecute(() =>
                {
                    app.IsInteractive = false;
                    return 5;
                });
            });

            var nonExistingCommandName = "NonExistingCommand";
            promptMock
                .SetupSequence(prompt => prompt.GetString(It.IsAny<string>(), null, It.IsAny<ConsoleColor?>(), It.IsAny<ConsoleColor?>()))
                .Returns(nonExistingCommandName)
                .Returns("quit");

            var result = app.ExecuteInteractive();

            Assert.Contains($"Unrecognized command or argument '{nonExistingCommandName}'", sb.ToString());
            Assert.NotEqual(5, result);
            Assert.False(app.IsInteractive);
        }

        [Fact]
        public void RethrowExceptionAfterException()
        {
            var promptMock = new Mock<IPrompt>();
            var sb = new StringBuilder();
            var output = new StringWriter(sb);
            var console = new TestConsole(_output)
            {
                Out = output,
            };

            var app = new CommandLineApplication(promptMock.Object, new InteractiveExecuteSettings() { ReThrowException = true }, console);

            var nonExistingCommandName = "NonExistingCommand";
            promptMock
                .Setup(prompt => prompt.GetString(It.IsAny<string>(), null, It.IsAny<ConsoleColor?>(), It.IsAny<ConsoleColor?>()))
                .Returns(nonExistingCommandName);

            Assert.ThrowsAny<UnrecognizedCommandParsingException>(() => app.ExecuteInteractive());
            Assert.DoesNotContain($"Unrecognized command or argument '{nonExistingCommandName}'", sb.ToString());
        }

        private class SimpleModel
        {
            private int OnExecute(CommandLineApplication application)
            {
                application.IsInteractive = false;
                return 5;
            }
        }

        [Fact]
        public void ExecuteWithStaticExecute()
        {
            var promptMock = new Mock<IPrompt>();
            promptMock
                .Setup(prompt => prompt.GetString(It.IsAny<string>(), null, It.IsAny<ConsoleColor?>(), It.IsAny<ConsoleColor?>()))
                .Returns("");

            var result = CommandLineApplication.ExecuteInteractive<SimpleModel>(prompt: promptMock.Object);

            Assert.Equal(5, result);
        }

    }
}
