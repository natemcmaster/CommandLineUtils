// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ResponseFileTests : IDisposable
    {
        private readonly string _tempDir;

        public ResponseFileTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        private string CreateResponseFile(params string[] lines)
        {
            var rsp = Path.Combine(_tempDir, Path.GetRandomFileName());
            File.WriteAllLines(rsp, lines);
            return rsp;
        }

        private List<string> ParseResponseFile(ResponseFileHandling options, params string[] responseFileLines)
        {
            var rsp = CreateResponseFile(responseFileLines);
            var app = new CommandLineApplication
            {
                ResponseFileHandling = options
            };
            var args = app.Argument("Words", "more words", multipleValues: true);
            Assert.Equal(0, app.Execute("@" + rsp));
            return args.Values;
        }

        [Fact]
        public void ItDoesNotParseResponseFilesByDefault()
        {
            var app = new CommandLineApplication();
            var args = app.Argument("Words", "more words", multipleValues: true);
            app.Execute("I", "don't", "@more.rsp");
            Assert.Equal(3, args.Values.Count);
            Assert.Equal("@more.rsp", args.Values[2]);
        }

        [Fact]
        public void ItParsesResponseFileAsSpaceSeparated()
        {
            var args = ParseResponseFile(ResponseFileHandling.ParseArgsAsSpaceSeparated,
                "Lorem",
                " ipsum ");

            Assert.Collection(args,
                a => Assert.Equal("Lorem", a),
                a => Assert.Equal("ipsum", a));
        }

        [Fact]
        public void ItParsesResponseFileAsLineSeparated()
        {
            var args = ParseResponseFile(ResponseFileHandling.ParseArgsAsLineSeparated,
                "Lorem ipsum",
                "   ",
                "",
                "# comment line",
                "  whitespace not changed ");

            Assert.Collection(args,
                a => Assert.Equal("Lorem ipsum", a),
                a => Assert.Equal("   ", a),
                a => Assert.Equal("", a),
                a => Assert.Equal("  whitespace not changed ", a));
        }

        [Fact]
        public void ItParsesResponseFileAsLineSeparatedSkipsTrailingEmptyLine()
        {
            var args = ParseResponseFile(ResponseFileHandling.ParseArgsAsLineSeparated,
                "Lorem ipsum",
                "");

            Assert.Collection(args,
                a => Assert.Equal("Lorem ipsum", a));
        }

        [Fact]
        public void ItThrowsCommandParsingException()
        {
            var app = new CommandLineApplication
            {
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
            };
            app.Argument("Input", "Input", multipleValues: true);

            var ex = Assert.Throws<CommandParsingException>(() => app.Execute("@somefilepath.rsp"));

            Assert.IsType<FileNotFoundException>(ex.InnerException);
        }

        [Fact]
        public void ItSkipsEmptyLines()
        {
            var args = ParseResponseFile(ResponseFileHandling.ParseArgsAsSpaceSeparated,
                "   ",
                "only",
                "   ",
                "");

            var arg = Assert.Single(args);
            Assert.Equal("only", arg);
        }

        [Fact]
        public void ItLoadsMultipleResponseFiles()
        {
            var rsp = CreateResponseFile(
                " Lorem ",
                " ipsum ");
            var app = new CommandLineApplication
            {
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsSpaceSeparated,
            };
            var args = app.Argument("Words", "more words", multipleValues: true);

            app.Execute("first", "@" + rsp, "middle", "@" + rsp, "end");

            Assert.Collection(args.Values,
                a => Assert.Equal("first", a),
                a => Assert.Equal("Lorem", a),
                a => Assert.Equal("ipsum", a),
                a => Assert.Equal("middle", a),
                a => Assert.Equal("Lorem", a),
                a => Assert.Equal("ipsum", a),
                a => Assert.Equal("end", a));
        }

        [Fact]
        public void ItSkipsLinesBeginningWithPound()
        {
            var args = ParseResponseFile(ResponseFileHandling.ParseArgsAsSpaceSeparated,
                "first",
                "# Skipped",
                "second");

            Assert.Collection(args,
                a => Assert.Equal("first", a),
                a => Assert.Equal("second", a));
        }

        [Fact]
        public void ItDoesNotLoadNestedResponseFiles()
        {
            var args = ParseResponseFile(ResponseFileHandling.ParseArgsAsSpaceSeparated, "@arg");
            var arg = Assert.Single(args);
            Assert.Equal("@arg", arg);
        }

        [Fact]
        public void ItSplitsLinesWithMultipleArgs()
        {
            var args = ParseResponseFile(ResponseFileHandling.ParseArgsAsSpaceSeparated,
                "first  second  third ",
                "a \"double quoted argument\"",
                "a 'single quoted argument'");

            Assert.Collection(args,
                a => Assert.Equal("first", a),
                a => Assert.Equal("second", a),
                a => Assert.Equal("third", a),
                a => Assert.Equal("a", a),
                a => Assert.Equal("double quoted argument", a),
                a => Assert.Equal("a", a),
                a => Assert.Equal("single quoted argument", a));
        }

        [Theory]
        [InlineData("a ' b c' z", new[] { "a", " b c", "z" })]
        [InlineData("a \" b c\" z", new[] { "a", " b c", "z" })]
        [InlineData("a '' b", new[] { "a", "", "b" })]
        [InlineData("a \"\" b", new[] { "a", "", "b" })]
        [InlineData("\"double quoted arg with 'single quotes'\"", new[] { "double quoted arg with 'single quotes'" })]
        [InlineData("'single quoted arg with \"double quotes\"'", new[] { "single quoted arg with \"double quotes\"" })]
        [InlineData("\"double quoted arg with escaped \\\" double quote\"", new[] { "double quoted arg with escaped \" double quote" })]
        [InlineData("'single quoted arg with escaped \\' single quote'", new[] { "single quoted arg with escaped ' single quote" })]
        [InlineData(@"\", new[] { @"\" })]
        [InlineData(@"\\", new[] { @"\\" })]
        [InlineData("\\\"", new[] { "\"" })]
        [InlineData("one\"arg not two\"", new[] { "onearg not two" })]
        [InlineData("one\"\"arg", new[] { "onearg" })]
        [InlineData("one\"\"arg\\\"withquote", new[] { "onearg\"withquote" })]
        [InlineData("''", new[] { "" })]
        [InlineData("'''", new[] { "" })]
        [InlineData("''''", new[] { "" })]
        [InlineData("\"\"", new[] { "" })]
        [InlineData("\"\"\"", new[] { "" })]
        [InlineData("\"\"\"\"", new[] { "" })]
        public void ItHandlesQuotedArgumentsInResponseFile(string input, string[] result)
        {
            var args = ParseResponseFile(ResponseFileHandling.ParseArgsAsSpaceSeparated, input);
            Assert.Equal(result, args);
        }

        [Fact]
        public void ItDoesNotAllowMultilinedQuotes()
        {
            var args = ParseResponseFile(ResponseFileHandling.ParseArgsAsSpaceSeparated,
                "\"double quoted",
                " that ends on next line\"");

            Assert.Collection(args,
                a => Assert.Equal("double quoted", a),
                a => Assert.Equal("that", a),
                a => Assert.Equal("ends", a),
                a => Assert.Equal("on", a),
                a => Assert.Equal("next", a),
                a => Assert.Equal("line", a));
        }

        [Fact]
        public void DoesNotLoadResponseFilesAfterArgumentSeparator()
        {
            var app = new CommandLineApplication
            {
                ThrowOnUnexpectedArgument = false,
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsSpaceSeparated,
                AllowArgumentSeparator = true,
            };
            app.Execute("--", "@somepath.txt");

            var arg = Assert.Single(app.RemainingArguments);
            Assert.Equal("@somepath.txt", arg);
        }

        [Fact]
        public void SubcommandsCanResponseFileOptions()
        {
            var app = new CommandLineApplication();
            CommandArgument wordArgs = null;
            app.Command("save", c =>
            {
                c.ResponseFileHandling = ResponseFileHandling.ParseArgsAsSpaceSeparated;
                wordArgs = c.Argument("words", "more words", multipleValues: true);
            });
            var rspFile = CreateResponseFile(" 'lorem ipsum' ", "dolor sit amet");
            app.Execute("save", "@" + rspFile);
            Assert.Collection(wordArgs.Values,
               a => Assert.Equal("lorem ipsum", a),
               a => Assert.Equal("dolor", a),
               a => Assert.Equal("sit", a),
               a => Assert.Equal("amet", a));
        }

        [Fact]
        public void HandlesResponseFilesWhenGivenAsOptionArg()
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsSpaceSeparated,
            };
            var opt = app.Option("--message <MESSAGE>", "Message", CommandOptionType.SingleValue);
            var rspFile = CreateResponseFile(" 'lorem ipsum' ", "dolor sit amet");
            app.Execute("--message", "@" + rspFile);

            Assert.Equal("lorem ipsum", opt.Value());

            Assert.Collection(app.RemainingArguments,
               a => Assert.Equal("dolor", a),
               a => Assert.Equal("sit", a),
               a => Assert.Equal("amet", a));
        }
    }
}
