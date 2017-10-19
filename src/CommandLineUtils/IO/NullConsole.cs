// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// An implementation of <see cref="IConsole"/> that does nothing.
    /// </summary>
    public class NullConsole : IConsole
    {
        private NullConsole()
        {
            Error = Out = new NullTextWriter();
        }

        /// <summary>
        /// A shared instance of <see cref="NullConsole"/>.
        /// </summary>
        public static NullConsole Singleton { get; } = new NullConsole();

        /// <summary>
        /// A writer that does nothing. 
        /// </summary>
        public TextWriter Out { get; }

        /// <summary>
        /// A writer that does nothing. 
        /// </summary>
        public TextWriter Error { get; }

        /// <summary>
        /// An empty reader.
        /// </summary>
        public TextReader In { get; } = new StringReader(string.Empty);

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsInputRedirected => false;

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsOutputRedirected => false;

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsErrorRedirected => false;

        /// <inheritdoc />
        public ConsoleColor ForegroundColor { get; set; }

        /// <inheritdoc />
        public ConsoleColor BackgroundColor { get; set; }

        /// <summary>
        /// This event never fires.
        /// </summary>
        public event ConsoleCancelEventHandler CancelKeyPress
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void ResetColor()
        {
        }

        private sealed class NullTextWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.Unicode;

            public override void Write(char value)
            {
            }
        }
    }
}
