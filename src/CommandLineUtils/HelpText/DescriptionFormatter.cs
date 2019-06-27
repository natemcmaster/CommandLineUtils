using System;

namespace McMaster.Extensions.CommandLineUtils.HelpText
{
    /// <summary>
    /// A formatter for creating nicely wrapped descriptions for display on the command line in the second column
    /// of generated help text.
    /// </summary>
    internal class DescriptionFormatter
    {
        private int _firstColumnWidth;
        private int _spacerLength;
        private int _consoleWidth;

        public DescriptionFormatter(int firstColumnWidth, int spacerLength, int? consoleWidth = null)
        {
            _firstColumnWidth = firstColumnWidth;
            _spacerLength = spacerLength;
            _consoleWidth = consoleWidth ?? Console.BufferWidth;
        }

        public string Wrap(string original)
        {
            return "";
        }
    }
}
