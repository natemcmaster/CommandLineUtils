// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandLineParserTests
    {
        private class Program
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
            var program = CommandLineParser.ParseArgs<Program>("commit", "-m", "Add attribute parsing", "--amend");

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
                () => CommandLineParser.ParseArgs<Program>("-f"));
            Assert.StartsWith("Unrecognized option '-f'", ex.Message);
        }

        private class PrivateSetterProgram
        {
            public int _value;
            public static int _staticvalue;

            [Option]
            public int Number { get; private set; }

            [Option]
            public int Count { get; }

            [Option]
            public int Value
            {
                get => _value;
                set => _value = value + 1;
            }

            [Option("--static-number")]
            public static int StaticNumber { get; private set; }

            [Option("--static-string")]
            public static string StaticString { get; }

            [Option("--static-value")]
            public static int StaticValue
            {
                get => _staticvalue;
                set => _staticvalue = value + 1;
            }
        }

        [Fact]
        public void BindsToPrivateSetProperties()
        {
            var parsed = CommandLineParser.ParseArgs<PrivateSetterProgram>("--number", "1");
            Assert.Equal(1, parsed.Number);
        }

        [Fact]
        public void BindsToReadOnlyProperties()
        {
            var parsed = CommandLineParser.ParseArgs<PrivateSetterProgram>("--count", "1");
            Assert.Equal(1, parsed.Count);
        }

        [Fact]
        public void BindsToPropertiesWithSetterMethod()
        {
            var parsed = CommandLineParser.ParseArgs<PrivateSetterProgram>("--value", "1");
            Assert.Equal(2, parsed.Value);
        }

        [Fact]
        public void BindsToStaticProperties()
        {
            CommandLineParser.ParseArgs<PrivateSetterProgram>("--static-number", "1");
            Assert.Equal(1, PrivateSetterProgram.StaticNumber);
        }

        [Fact]
        public void BindsToStaticReadOnlyProps()
        {
            CommandLineParser.ParseArgs<PrivateSetterProgram>("--static-string", "1");
            Assert.Equal("1", PrivateSetterProgram.StaticString);
        }

        [Fact]
        public void BindsToStaticPropertiesWithSetterMethod()
        {
            CommandLineParser.ParseArgs<PrivateSetterProgram>("--static-value", "1");
            Assert.Equal(2, PrivateSetterProgram.StaticValue);
        }
    }
}
