// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using McMaster.Extensions.CommandLineUtils.Internal;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class TestConsole : IConsole, ICancellationTokenProvider
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

        CancellationToken ICancellationTokenProvider.Token => CancelKeyPressToken;

        public CancellationToken CancelKeyPressToken => CancelKeyCancellationSource.Token;

        public CancellationTokenSource CancelKeyCancellationSource { get; } = new CancellationTokenSource();

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
