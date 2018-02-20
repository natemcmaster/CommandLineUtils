using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Selection keys
    /// </summary>
    public class OptionsKeys
    {
        /// <summary>
        /// Gets or sets the select keys.
        /// </summary>
        /// <value>
        /// The select keys.
        /// </value>
        public ConsoleKey[] Select { get; set; } = { ConsoleKey.Spacebar, ConsoleKey.X };

        /// <summary>
        /// Gets or sets the move down keys.
        /// </summary>
        /// <value>
        /// The move down keys.
        /// </value>
        public ConsoleKey[] Movedown { get; set; } = { ConsoleKey.DownArrow };

        /// <summary>
        /// Gets or sets the move up keys.
        /// </summary>
        /// <value>
        /// The move up keys.
        /// </value>
        public ConsoleKey[] Moveup { get; set; } = { ConsoleKey.UpArrow };

        /// <summary>
        /// Gets or sets the "end" keys.
        /// </summary>
        /// <value>
        /// The "end" keys.
        /// </value>
        public ConsoleKey[] Finalize { get; set; } = { ConsoleKey.Enter };
    }
}