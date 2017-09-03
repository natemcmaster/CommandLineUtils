// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.IO;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// An implementation of <see cref="IConsole"/> that wraps <see cref="System.Console"/>.
    /// </summary>
    public class PhysicalConsole : IConsole
    {
        /// <summary>
        /// A shared instance of <see cref="PhysicalConsole"/>.
        /// </summary>
        public static IConsole Singleton { get; } = new PhysicalConsole();

        /// <summary>
        /// <see cref="Console.CancelKeyPress"/>.
        /// </summary>
        public event ConsoleCancelEventHandler CancelKeyPress
        {
            add => Console.CancelKeyPress += value;
            remove => Console.CancelKeyPress -= value;
        }

        /// <summary>
        /// <see cref="Console.Error"/>.
        /// </summary>
        public TextWriter Error => Console.Error;

        /// <summary>
        /// <see cref="Console.In"/>.
        /// </summary>
        public TextReader In => Console.In;

        /// <summary>
        /// <see cref="Console.Out"/>.
        /// </summary>
        public TextWriter Out => Console.Out;

        /// <summary>
        /// <see cref="Console.IsInputRedirected"/>.
        /// </summary>
        public bool IsInputRedirected => Console.IsInputRedirected;

        /// <summary>
        /// <see cref="Console.IsOutputRedirected"/>.
        /// </summary>
        public bool IsOutputRedirected => Console.IsOutputRedirected;

        /// <summary>
        /// <see cref="Console.IsErrorRedirected"/>.
        /// </summary>
        public bool IsErrorRedirected => Console.IsErrorRedirected;

        /// <summary>
        /// <see cref="Console.ForegroundColor"/>.
        /// </summary>
        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        /// <summary>
        /// <see cref="Console.BackgroundColor"/>.
        /// </summary>
        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }

        /// <summary>
        /// <see cref="Console.ResetColor"/>.
        /// </summary>
        public void ResetColor() => Console.ResetColor();
    }
}
