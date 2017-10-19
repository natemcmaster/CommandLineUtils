// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class XunitTextWriter : TextWriter
    {
        private readonly ITestOutputHelper _output;
        private readonly StringBuilder _sb = new StringBuilder();

        public XunitTextWriter(ITestOutputHelper output)
        {
            _output = output;
        }

        public override Encoding Encoding => Encoding.Unicode;

        public override void Write(char ch)
        {
            if (ch == '\n')
            {
                _output.WriteLine(_sb.ToString());
                _sb.Clear();
            }
            else
            {
                _sb.Append(ch);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sb.Length > 0)
                {
                    _output.WriteLine(_sb.ToString());
                    _sb.Clear();
                }
            }

            base.Dispose(disposing);
        }
    }
}
