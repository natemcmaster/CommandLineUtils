// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils.HelpText;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class HangingIndentWriterTests
    {
        private const int IndentSize = 20;
        private const int ConsoleWidth = 40; // A very skinny console for testing.
        private string _paddedNewline = Environment.NewLine + new string(' ', IndentSize);

        private HangingIndentWriter Subject()
        {
            return new HangingIndentWriter(IndentSize, ConsoleWidth);
        }

        private void AssertWrapBehavior(string original, string expected)
        {
            Subject().Write(original).Should().Be(expected);
        }

        [Fact]
        public void EmptReturnsEmpty()
        {
            AssertWrapBehavior("", "");
        }

        [Fact]
        public void SimpleOneLineJustWorks()
        {
            var originalText = "Verbosity setting";
            var expected = "Verbosity setting";
            AssertWrapBehavior(originalText, expected);
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

            AssertWrapBehavior(originalText, expected);
        }

        [Fact]
        public void TheFirstLineCanAlsoBeIndented()
        {
            var originalText = "This argument description is really long. It is a great argument. The best argument.";
            var expected =
                _paddedNewline + "This argument" +
                _paddedNewline + "description is" +
                _paddedNewline + "really long. It is a" +
                _paddedNewline + "great argument. The" +
                _paddedNewline + "best argument.";

            var subject = new HangingIndentWriter(IndentSize, maxLineLength: ConsoleWidth, indentFirstLine: true);
            subject.Write(originalText).Should().Be(expected);
        }

        [Fact]
        public void LongRunningFirstLineStaysOnFirstLine()
        {
            var originalText = "SomeReallyLongWordWithNoSpacesAtAll the end.";
            var expected = "SomeReallyLongWordWithNoSpacesAtAll" +
                _paddedNewline + "the end.";

            AssertWrapBehavior(originalText, expected);
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

            AssertWrapBehavior(originalText, expected);
        }

        [Theory]
        [InlineData("You can have\n\ndouble lines.")]
        [InlineData("You can have\r\n\r\ndouble lines.")]
        public void DoubleLinesArePreserved(string originalText)
        {
            var expected = "You can have" +
                _paddedNewline +
                _paddedNewline + "double lines.";

            AssertWrapBehavior(originalText, expected);
        }
    }
}
