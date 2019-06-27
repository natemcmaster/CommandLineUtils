using FluentAssertions;
using McMaster.Extensions.CommandLineUtils.HelpText;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class DescriptionFormatterTests
    {
        private const int FirstColumnWidth = 18;
        private const int SpacerLength = 2;
        private const int ConsoleWidth = 40; // A very skinny console for testing.

        // Always using \n in our wrapped output as Consoles in Windows these days are fine with this.
        // As of RS5 so is notepad.exe on Windows.
        private string _paddedNewline = $"\n{new string(' ', FirstColumnWidth + SpacerLength)}";

        private DescriptionFormatter Subject()
        {
            return new DescriptionFormatter(FirstColumnWidth, SpacerLength, ConsoleWidth);
        }

        private void AssertExpected(string original, string expected)
        {
            Subject().Wrap(original).Should().Be(expected);
        }

        [Fact]
        public void SimpleOneLineJustWorks()
        {
            var originalText = "Verbosity setting";
            var expected = "Verbosity setting";
            AssertExpected(originalText, expected);
        }

        [Fact]
        public void ItWrapsADescriptionBasedOnConsoleWidthAndFirstColumnSize()
        {
            var originalText = "This argument description is really long. It is a great argument. The best argument.";
            var expected = "This argument" +
                _paddedNewline + "description is" +
                _paddedNewline + "really long. It is a" +
                _paddedNewline + "great argument. The" +
                _paddedNewline + "best argument.";

            AssertExpected(originalText, expected);
        }

        [Fact]
        public void LongRunningFirstLineStaysOnFirstLine()
        {
            var originalText = "SomeReallyLongWordWithNoSpacesAtAll the end.";
            var expected = "SomeReallyLongWordWithNoSpacesAtAll" +
                _paddedNewline + "the end.";

            AssertExpected(originalText, expected);
        }

        [Theory]
        [InlineData("I want\nmy own newlines\nplease the end.\nBut long lines do still get broken up correctly!")]
        [InlineData("I want\r\nmy own newlines\r\nplease the end.\r\nBut long lines do still get broken up correctly!")]
        public void ExplicitNewLinesArePreservedRegardlessOfType(string originalText)
        {
            var expected = "I want" +
                _paddedNewline + "my own newlines" +
                _paddedNewline + "please the end." +
                _paddedNewline + "But long lines do" +
                _paddedNewline + "still get broken up" +
                _paddedNewline + "correctly!";

            AssertExpected(originalText, expected);
        }

        [Theory]
        [InlineData("You can have\n\ndouble lines.")]
        [InlineData("You can have\r\n\r\ndouble lines.")]
        public void DoubleLinesArePreserved(string originalText)
        {
            var expected = "You can have" +
                _paddedNewline +
                _paddedNewline + "double lines.";

            AssertExpected(originalText, expected);
        }
    }
}
