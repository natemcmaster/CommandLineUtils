// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.IO;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// A thread-safe reporter that forwards to console output.
    /// </summary>
    public class ConsoleReporter : IReporter
    {
        private object _writeLock = new object();

        /// <summary>
        /// Initializes an instance of <see cref="ConsoleReporter"/>.
        /// </summary>
        /// <param name="console"></param>
        public ConsoleReporter(IConsole console)
            : this(console, verbose: false, quiet: false)
        { }

        /// <summary>
        /// Initializes an instance of <see cref="ConsoleReporter"/>.
        /// </summary>
        /// <param name="console"></param>
        /// <param name="verbose">When false, Verbose does not display output.</param>
        /// <param name="quiet">When true, only Warn and Error display output</param>
        public ConsoleReporter(IConsole console, bool verbose, bool quiet)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
            IsVerbose = verbose;
            IsQuiet = quiet;
        }

        /// <summary>
        /// The console to write to.
        /// </summary>
        protected IConsole Console { get; }

        /// <summary>
        /// Is verbose output displayed.
        /// </summary>
        public bool IsVerbose { get; set; }

        /// <summary>
        /// Is verbose output and regular output hidden.
        /// </summary>
        public bool IsQuiet { get; set; }

        /// <summary>
        /// Write a line with color.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="message"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="backgroundColor"></param>
        protected virtual void WriteLine(TextWriter writer, string message, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor = default(ConsoleColor?))
        {
            lock (_writeLock)
            {
                if (foregroundColor.HasValue)
                {
                    Console.ForegroundColor = foregroundColor.Value;
                }

                if (backgroundColor.HasValue)
                {
                    Console.BackgroundColor = backgroundColor.Value;
                }

                writer.WriteLine(message);

                if (foregroundColor.HasValue)
                {
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Writes a message in <see cref="ConsoleColor.Red"/> to <see cref="IConsole.Error"/>.
        /// </summary>
        /// <param name="message"></param>
        public virtual void Error(string message)
            => WriteLine(Console.Error, message, ConsoleColor.Red);

        /// <summary>
        /// Writes a message in <see cref="ConsoleColor.Yellow"/> to <see cref="IConsole.Out"/>.
        /// </summary>
        /// <param name="message"></param>
        public virtual void Warn(string message)
            => WriteLine(Console.Out, message, ConsoleColor.Yellow);

        /// <summary>
        /// Writes a message to <see cref="IConsole.Out"/>.
        /// </summary>
        /// <param name="message"></param>
        public virtual void Output(string message)
        {
            if (IsQuiet)
            {
                return;
            }
            WriteLine(Console.Out, message, foregroundColor: null);
        }

        /// <summary>
        /// Writes a message in <see cref="ConsoleColor.DarkGray"/> to <see cref="IConsole.Out"/>.
        /// </summary>
        /// <param name="message"></param>
        public virtual void Verbose(string message)
        {
            if (!IsVerbose)
            {
                return;
            }

            WriteLine(Console.Out, message, ConsoleColor.DarkGray);
        }
    }
}
