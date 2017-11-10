// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class TestConsole : IConsole
    {
        private readonly ITestOutputHelper _output;

        public TestConsole(ITestOutputHelper output)
        {
            _output = output;
        }

        public TextWriter Out => new XunitTextWriter(_output);

        public TextWriter Error => new XunitTextWriter(_output);

        public TextReader In => throw new NotImplementedException();

        public bool IsInputRedirected => throw new NotImplementedException();

        public bool IsOutputRedirected => throw new NotImplementedException();

        public bool IsErrorRedirected => throw new NotImplementedException();

        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }

        public event ConsoleCancelEventHandler CancelKeyPress
        {
            add { }
            remove { }
        }

        public void ResetColor()
        {
        }
    }
}
