// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCOREAPP2_0
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class DotNetExeTests
    {
        [Fact]
        public void FindsTheDoNetPath()
        {
            var dotnetPath = DotNetExe.FullPath;
            Assert.NotNull(dotnetPath);
            Assert.True(File.Exists(dotnetPath), "The file did not exist");
            Assert.True(Path.IsPathRooted(dotnetPath), "The path should be rooted");
            Assert.Equal("dotnet", Path.GetFileNameWithoutExtension(dotnetPath), ignoreCase: true);
        }
    }
}
#endif
