using FluentAssertions;
using McMaster.Extensions.CommandLineUtils.HelpText;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class DescriptionFormatterTests
    {
        [Fact]
        public void ItWrapsADescriptionBasedOnConsoleWidthAndFirstColumnSize()
        {
            var originalText = "This argument description is really long. It is a great argument. The best argument.";
            var spacerLength = 2;
            var firstColumnWidth = 18;
            var consoleWidth = 40; // Super skinny console.
            var paddedNewline = $"\n{new string(' ', firstColumnWidth + spacerLength)}";

            var expected = "This argument" +
                paddedNewline + "description is" +
                paddedNewline + "really long. It is a" +
                paddedNewline + "great argument. The" +
                paddedNewline + "best argument.";

            var subject = new DescriptionFormatter(firstColumnWidth, spacerLength, consoleWidth: consoleWidth);

            var result = subject.Wrap(originalText);

            result.Should().Be(expected);
        }
    }
}
