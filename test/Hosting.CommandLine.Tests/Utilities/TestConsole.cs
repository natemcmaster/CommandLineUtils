// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Xunit.Abstractions;

namespace McMaster.Extensions.Hosting.CommandLine.Tests.Utilities
{
    public class TestConsole : IConsole
    {
        public TestConsole(ITestOutputHelper output)
        {
            Out = new XunitTextWriter(output);
            Error = new XunitTextWriter(output);
        }

        public TextWriter Out { get; set; }

        public TextWriter Error { get; set; }

        public TextReader In => throw new NotImplementedException();

        public bool IsInputRedirected => throw new NotImplementedException();

        public bool IsOutputRedirected => true;

        public bool IsErrorRedirected => true;

        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }

        public event ConsoleCancelEventHandler? CancelKeyPress
        {
            add { }
            remove { }
        }

        public void ResetColor()
        {
        }
    }
}
