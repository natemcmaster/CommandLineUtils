// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils.HelpText
{
    /// <summary>
    /// A formatter for creating nicely wrapped descriptions for display on the command line in the second column
    /// of generated help text.
    /// </summary>
    public class HangingIndentWriter
    {
        /// <summary>
        /// The default console width used for wrapping if the width cannot be gotten from the Console.
        /// </summary>
        public const int DefaultConsoleWidth = 80;

        private readonly bool _indentFirstLine;
        private readonly int _indentSize;
        private readonly int _maxLineLength;
        private readonly string _paddedLine;

        /// <summary>
        /// A description formatter for dynamically wrapping the description to print in a CLI usage.
        /// </summary>
        /// <param name="indentSize">The indent size in spaces to use.</param>
        /// <param name="maxLineLength">The max length an indented line can be.
        /// Defaults to <see cref="DefaultConsoleWidth"/>.
        /// </param>
        /// <param name="indentFirstLine">If true, the first line of text will also be indented.</param>
        public HangingIndentWriter(int indentSize, int? maxLineLength = null, bool indentFirstLine = false)
        {
            _indentSize = indentSize;
            _maxLineLength = maxLineLength ?? DefaultConsoleWidth;
            _indentFirstLine = indentFirstLine;
            _paddedLine = Environment.NewLine + new string(' ', _indentSize);
        }

        /// <summary>
        /// Dynamically wrap text between.
        /// </summary>
        /// <param name="input">The original description text.</param>
        /// <returns>Dynamically wrapped description with explicit newlines preserved.</returns>
        public string Write(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var lines = input
                .Split(new[] { "\n", "\r\n" }, StringSplitOptions.None)
                .Select(WrapSingle);

            return (_indentFirstLine ? _paddedLine : string.Empty) + string.Join(_paddedLine, lines);
        }

        /// <summary>
        /// Wrap a single line based on console width.
        /// </summary>
        /// <param name="original">The original description text.</param>
        /// <returns>Description text wrapped with padded newlines.</returns>
        private string WrapSingle(string original)
        {
            StringBuilder sb = new StringBuilder();
            var lineLength = _indentSize;
            foreach (var token in original.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (lineLength == _indentSize)
                {
                    // At the beginning of a new padded line, just append token.
                }
                else if (lineLength + 1 + token.Length > _maxLineLength)
                {
                    // Adding a space + token would push over console width, need a new line.
                    sb.Append(_paddedLine);
                    lineLength = _indentSize;
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
