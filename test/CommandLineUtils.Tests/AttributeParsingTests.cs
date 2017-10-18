using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class AttributeParsingTests
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
            var program = CommandLineApplication.ParseArgs<Program>("commit", "-m", "Add attribute parsing", "--amend");

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
                () => CommandLineApplication.ParseArgs<Program>("-f"));
            Assert.StartsWith("Unrecognized option '-f'", ex.Message);
        }
    }
}
