// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This file has been modified from the original form. See Notice.txt in the project root for more information.

#if NET6_0_OR_GREATER
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class DotNetExeTests
    {
        [Fact]
        public void FindsTheDotNetPath()
        {
            var dotnetPath = DotNetExe.FullPath;
            Assert.NotNull(dotnetPath);
            Assert.True(File.Exists(dotnetPath), "The file did not exist");
            Assert.True(Path.IsPathRooted(dotnetPath), "The path should be rooted");
            Assert.Equal("dotnet", Path.GetFileNameWithoutExtension(dotnetPath), ignoreCase: true);
        }

        [Fact]
        public void FullPathOrDefaultReturnsPathOrDotnet()
        {
            var result = DotNetExe.FullPathOrDefault();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // Should either be a rooted path that exists, or just "dotnet"
            if (Path.IsPathRooted(result))
            {
                Assert.True(File.Exists(result), "The file did not exist");
            }
            else
            {
                Assert.Equal("dotnet", result);
            }
        }

        [Fact]
        public void FindDotNetInRoot_ReturnsNull_WhenDirectoryDoesNotExist()
        {
            var fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
            var result = DotNetExe.FindDotNetInRoot("/nonexistent/path/that/does/not/exist", fileName);
            Assert.Null(result);
        }

        [Fact]
        public void FindDotNetInRoot_ReturnsNull_WhenFileDoesNotExist()
        {
            // Use a directory that exists but won't contain dotnet
            var tempDir = Path.GetTempPath();
            var result = DotNetExe.FindDotNetInRoot(tempDir, "dotnet-nonexistent-file");
            Assert.Null(result);
        }

        [Fact]
        public void FindDotNetInRoot_ReturnsPath_WhenFileExists()
        {
            // Create a temp file to simulate dotnet existing
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var fakeDotnet = "dotnet-test";
                var fakePath = Path.Combine(tempDir, fakeDotnet);
                File.WriteAllText(fakePath, "");

                var result = DotNetExe.FindDotNetInRoot(tempDir, fakeDotnet);
                Assert.NotNull(result);
                Assert.Equal(fakePath, result);
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
#elif NET472_OR_GREATER
#else
#error Update target frameworks
#endif
