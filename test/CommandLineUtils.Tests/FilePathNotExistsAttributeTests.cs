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
    public class FilePathNotExistsAttributeTests
    {
        private readonly ITestOutputHelper _output;

        public FilePathNotExistsAttributeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class App
        {
            [Argument(0)]
            [FileOrDirectoryNotExists]
            public string? File { get; }

            private void OnExecute() { }
        }

        [Theory]
        [InlineData("exists.txt")]
        public void ValidatesFilesMustNotExist(string filePath)
        {
            var exists = Path.Combine(AppContext.BaseDirectory, filePath);
            if (!File.Exists(exists))
            {
                File.WriteAllText(exists, "");
            }

            var app = new CommandLineApplication(
                new TestConsole(_output),
                AppContext.BaseDirectory);

            app.Argument("Files", "Files")
                .Accepts().NonExistingFileOrDirectory();

            var result = app
                .Parse(filePath)
                .SelectedCommand
                .GetValidationResult();

            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal($"The file path '{filePath}' already exists.", result.ErrorMessage);

            var console = new TestConsole(_output);
            Assert.NotEqual(0, CommandLineApplication.Execute<App>(console, filePath));
        }

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
                .Accepts(v => v.NonExistingFileOrDirectory());
            appNotInBaseDir.Argument("Files", "Files")
                .Accepts(v => v.NonExistingFileOrDirectory());

            var fails = appInBaseDir
                .Parse("exists.txt")
                .SelectedCommand
                .GetValidationResult();

            var success = appNotInBaseDir
                .Parse("exists.txt")
                .SelectedCommand
                .GetValidationResult();

            Assert.NotEqual(ValidationResult.Success, fails);
            Assert.Equal("The file path 'exists.txt' already exists.", fails.ErrorMessage);

            Assert.Equal(ValidationResult.Success, success);

            var console = new TestConsole(_output);
            var context = new DefaultCommandLineContext(console, appInBaseDir.WorkingDirectory, new[] { "exists.txt" });
            Assert.NotEqual(0, CommandLineApplication.Execute<App>(context));

            context = new DefaultCommandLineContext(console, appNotInBaseDir.WorkingDirectory, new[] { "exists.txt" });
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

            Assert.NotEqual(0, CommandLineApplication.Execute<App>(context));
        }

        private class OnlyDir
        {
            [Argument(0)]
            [DirectoryNotExists]
            public string? Dir { get; }

            private void OnExecute() { }
        }

        private class OnlyFile
        {
            [Argument(0)]
            [FileNotExists]
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

            Assert.Equal(0, CommandLineApplication.Execute<OnlyFile>(context));
            Assert.NotEqual(0, CommandLineApplication.Execute<OnlyDir>(context));
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

            Assert.NotEqual(0, CommandLineApplication.Execute<OnlyFile>(context));
            Assert.Equal(0, CommandLineApplication.Execute<OnlyDir>(context));
        }
    }
}
