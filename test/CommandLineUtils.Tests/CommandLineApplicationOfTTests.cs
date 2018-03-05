// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandLineApplicationOfTTests
    {
        private class AttributesNotUsedClass
        {
            public int OptionA { get; set; }
            public string OptionB { get; set; }
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
            public string Command { get; set; }

            [Option]
            public string Message { get; set; }

            [Option("-F <file>")]
            public string File { get; set; }

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
            var ex = Assert.Throws<CommandParsingException>(
                () => CommandLineParser.ParseArgs<SimpleProgram>("-f"));
            Assert.StartsWith("Unrecognized option '-f'", ex.Message);
        }
    }
}
