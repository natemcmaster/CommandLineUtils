// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class LegalFilePathAttributeTests
    {
        private readonly ITestOutputHelper _output;

        public LegalFilePathAttributeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class App
        {
            [Argument(0), LegalFilePath]
            public string? FilePath { get; set; }
            private void OnExecute() { }
        }

        [Theory]
        [InlineData(@"C:\dir")]
        [InlineData(@"/dir")]
        [InlineData(@"../dir")]
        [InlineData(@"dir")]
        [InlineData(@"file.txt")]
        [InlineData(@".file")]
        [InlineData(@"/file.txt")]
        [InlineData(@"C:\file.txt")]
        [InlineData(@"..")]
        [InlineData(@".")]
        [InlineData(@"./")]
        [InlineData(@"../")]
        [InlineData(@"../..")]
        public void ValidatesLegalFilePaths(string filePath)
        {
            var console = new TestConsole(_output);
            Assert.Equal(0, CommandLineApplication.Execute<App>(console, filePath));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\0")]
        public void FailsInvalidLegalFilePaths(string filePath)
        {
            var console = new TestConsole(_output);
            Assert.NotEqual(0, CommandLineApplication.Execute<App>(console, filePath));
        }
    }
}
