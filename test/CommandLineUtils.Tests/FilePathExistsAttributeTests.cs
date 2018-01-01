// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class FilePathExistsAttributeTests
    {
        private readonly ITestOutputHelper _output;

        public FilePathExistsAttributeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [MemberData(nameof(BadFilePaths))]
        public void ValidatesFilesMustExist(string filePath)
        {
            var app = new CommandLineApplication(
                new TestConsole(_output),
                AppContext.BaseDirectory, false);

            app.Argument("Files", "Files")
                .Accepts(v => v.IsExistingFilePath());

            var result = new CommandLineProcessor(app, new[] { filePath })
                .Process()
                .GetValidationResult();

            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal($"The file path '{filePath}' does not exist.", result.ErrorMessage);
        }

        public static TheoryData<string> BadFilePaths
            => new TheoryData<string>
            {
                "notfound.txt",
                "\0",
                null,
                string.Empty,
            };

        [Fact]
        public void ValidatesFilesRelativeToAppContext()
        {
            var exists = Path.Combine(AppContext.BaseDirectory, "exists.txt");
            if (!File.Exists(exists))
            {
                File.WriteAllText(exists, "");
            }

            var appInBaseDir = new CommandLineApplication(
                new TestConsole(_output),
                AppContext.BaseDirectory,
                false);
            var notFoundDir = Path.Combine(AppContext.BaseDirectory, "notfound");
            var appNotInBaseDir = new CommandLineApplication(
               new TestConsole(_output),
               notFoundDir,
               false);

            appInBaseDir.Argument("Files", "Files")
                .Accepts(v => v.IsExistingFilePath());
            appNotInBaseDir.Argument("Files", "Files")
                .Accepts(v => v.IsExistingFilePath());

            var success = new CommandLineProcessor(appInBaseDir, new[] { "exists.txt" })
                .Process()
                .GetValidationResult();

            var fails = new CommandLineProcessor(appNotInBaseDir, new[] { "exists.txt" })
                .Process()
                .GetValidationResult();

            Assert.Equal(ValidationResult.Success, success);

            Assert.NotEqual(ValidationResult.Success, fails);
            Assert.Equal("The file path 'exists.txt' does not exist.", fails.ErrorMessage);
        }
    }
}
