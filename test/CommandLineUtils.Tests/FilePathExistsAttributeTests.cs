// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils.Internal;
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

        private class App
        {
            [Argument(0)]
            [FileOrDirectoryExists]
            public string? File { get; }

            private void OnExecute() { }
        }

        [Theory]
        [MemberData(nameof(BadFilePaths))]
        public void ValidatesFilesMustExist(string? filePath)
        {
            var app = new CommandLineApplication(
                new TestConsole(_output),
                AppContext.BaseDirectory);

            app.Argument("Files", "Files")
                .Accepts().ExistingFileOrDirectory();

            var result = app
                .Parse(filePath!)
                .SelectedCommand
                .GetValidationResult();

            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal($"The file path '{filePath}' does not exist.", result.ErrorMessage);

            var console = new TestConsole(_output);
            Assert.NotEqual(0, CommandLineApplication.Execute<App>(console, filePath!));
        }

        public static TheoryData<string?> BadFilePaths
            => new TheoryData<string?>
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
                AppContext.BaseDirectory);
            var notFoundDir = Path.Combine(AppContext.BaseDirectory, "notfound");
            var appNotInBaseDir = new CommandLineApplication(
               new TestConsole(_output),
               notFoundDir);

            appInBaseDir.Argument("Files", "Files")
                .Accepts(v => v.ExistingFileOrDirectory());
            appNotInBaseDir.Argument("Files", "Files")
                .Accepts(v => v.ExistingFileOrDirectory());

            var success = appInBaseDir
                .Parse("exists.txt")
                .SelectedCommand
                .GetValidationResult();

            var fails = appNotInBaseDir
                .Parse("exists.txt")
                .SelectedCommand
                .GetValidationResult();

            Assert.Equal(ValidationResult.Success, success);

            Assert.NotEqual(ValidationResult.Success, fails);
            Assert.Equal("The file path 'exists.txt' does not exist.", fails.ErrorMessage);

            var console = new TestConsole(_output);
            var context = new DefaultCommandLineContext(console, appNotInBaseDir.WorkingDirectory, new[] { "exists.txt" });
            Assert.NotEqual(0, CommandLineApplication.Execute<App>(context));

            context = new DefaultCommandLineContext(console, appInBaseDir.WorkingDirectory, new[] { "exists.txt" });
            Assert.Equal(0, CommandLineApplication.Execute<App>(context));
        }

        [Theory]
        [InlineData("./dir")]
        [InlineData("./")]
        [InlineData("../")]
        public void ValidatesDirectories(string dirPath)
        {
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, dirPath));

            var context = new DefaultCommandLineContext(
                new TestConsole(_output),
                AppContext.BaseDirectory,
                new[] { dirPath });

            Assert.Equal(0, CommandLineApplication.Execute<App>(context));
        }

        private class OnlyDir
        {
            [Argument(0)]
            [DirectoryExists]
            public string? Dir { get; }

            private void OnExecute() { }
        }

        private class OnlyFile
        {
            [Argument(0)]
            [FileExists]
            public string? Path { get; }

            private void OnExecute() { }
        }

        [Theory]
        [InlineData("./dir")]
        [InlineData("./")]
        [InlineData("../")]
        public void ValidatesOnlyDirectories(string dirPath)
        {
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, dirPath));

            var context = new DefaultCommandLineContext(
                new TestConsole(_output),
                AppContext.BaseDirectory,
                new[] { dirPath });

            Assert.NotEqual(0, CommandLineApplication.Execute<OnlyFile>(context));
            Assert.Equal(0, CommandLineApplication.Execute<OnlyDir>(context));
        }

        [Fact]
        public void ValidatesOnlyFiles()
        {
            var filePath = "exists.txt";
            var fullPath = Path.Combine(AppContext.BaseDirectory, filePath);
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, "");
            }

            var context = new DefaultCommandLineContext(
                new TestConsole(_output),
                AppContext.BaseDirectory,
                new[] { filePath });

            Assert.Equal(0, CommandLineApplication.Execute<OnlyFile>(context));
            Assert.NotEqual(0, CommandLineApplication.Execute<OnlyDir>(context));
        }
    }
}
