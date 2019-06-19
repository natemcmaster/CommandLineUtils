// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandLineApplicationOfTTests
    {
        private readonly ITestOutputHelper _output;

        public CommandLineApplicationOfTTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class AttributesNotUsedClass
        {
            public int OptionA { get; set; }
            public string? OptionB { get; set; }
        }

        [Fact]
        public void AttributesAreRequired()
        {
            var app = new CommandLineApplication<AttributesNotUsedClass>();
            app.Conventions.UseAttributes();
            Assert.Empty(app.Arguments);
            Assert.Empty(app.Commands);
            Assert.Empty(app.Options);
            Assert.Null(app.OptionHelp);
            Assert.Null(app.OptionVersion);
        }


        private class SimpleProgram
        {
            [Argument(0)]
            public string? Command { get; set; }

            [Option]
            public string? Message { get; set; }

            [Option("-F <file>")]
            public string? File { get; set; }

            [Option]
            public bool Amend { get; set; }

            [Option("--no-edit")]
            public bool NoEdit { get; set; }
        }

        [Fact]
        public void InitializesTypeWithAttributes()
        {
            var program = CommandLineParser.ParseArgs<SimpleProgram>("commit", "-m", "Add attribute parsing", "--amend");

            Assert.NotNull(program);
            Assert.Equal("commit", program.Command);
            Assert.Equal("Add attribute parsing", program.Message);
            Assert.True(program.Amend);
            Assert.False(program.NoEdit);
        }

        [Fact]
        public void ThrowsForArgumentsWithoutMatchingAttribute()
        {
            var ex = Assert.ThrowsAny<CommandParsingException>(
                () => CommandLineParser.ParseArgs<SimpleProgram>(_output, "-f"));
            Assert.StartsWith("Unrecognized option '-f'", ex.Message);
        }

        private class TestClass
        {
            public TestClass(string _) { }
        }

        [Fact]
        public void ThrowsForNoParameterlessConstructor()
        {
            var app = new CommandLineApplication<TestClass>();
            var exception = Assert.Throws<MissingParameterlessConstructorException>(() => app.Execute());
            Assert.Equal(typeof(TestClass), exception.Type);
        }
    }
}
