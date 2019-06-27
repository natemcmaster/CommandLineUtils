using System;
using System.Linq;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils.HelpText
{
    /// <summary>
    /// A formatter for creating nicely wrapped descriptions for display on the command line in the second column
    /// of generated help text.
    /// </summary>
    internal class DescriptionFormatter
    {
        /// <summary>
        /// The length of the first column width plus the column spacer length.
        /// </summary>
        private int _paddedLineLength;

        private int _consoleWidth;
        private string _paddedLine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstColumnWidth">The width of the first column where the option/argument/command is listed.</param>
        /// <param name="spacerLength">The number of spaces separating the columns.</param>
        /// <param name="consoleWidth">The max width of the console controlling when to wrap to a new line.
        /// Defaults to Console.BufferWidth.
        /// </param>
        public DescriptionFormatter(int firstColumnWidth, int spacerLength, int? consoleWidth = null)
        {
            _paddedLineLength = firstColumnWidth + spacerLength;
            _consoleWidth = consoleWidth ?? Console.BufferWidth;
            _paddedLine = "\n" + new string(' ', _paddedLineLength);
        }

        public string Wrap(string original)
        {
            var lines = original
                .Split(new[] { "\n", "\r\n" }, StringSplitOptions.None)
                .Select(WrapSingle);

            return string.Join(_paddedLine, lines);
        }

        public string WrapSingle(string original)
        {
            StringBuilder sb = new StringBuilder();
            var lineLength = _paddedLineLength;
            foreach (var token in original.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (lineLength == _paddedLineLength)
                {
                    // At the beginning of a new padded line, just append token.
                }
                else if (lineLength + 1 + token.Length > _consoleWidth)
                {
                    // Adding a space + token would push over console width, need a new line.
                    sb.Append(_paddedLine);
                    lineLength = _paddedLineLength;
                }
                else
                {
                    // We can add a space and the token so add the space.
                    sb.Append(' ');
                    lineLength++;
                }

                sb.Append(token);
                lineLength += token.Length;
            }
            return sb.ToString();
        }
    }
}
