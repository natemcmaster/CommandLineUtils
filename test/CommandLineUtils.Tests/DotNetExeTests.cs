// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

#if (NETCOREAPP1_1 || NETCOREAPP2_0)
using System.IO;
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
    }
}
#elif NET461
#else
#error Update target frameworks
#endif
