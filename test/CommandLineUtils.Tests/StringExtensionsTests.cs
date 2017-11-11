// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("Option123", "option123")]
        [InlineData("dWORD", "d-word")]
        [InlineData("MSBuild", "msbuild")]
        [InlineData("NoEdit", "no-edit")]
        [InlineData("SetUpstreamBranch", "set-upstream-branch")]
        [InlineData("lowerCaseFirst", "lower-case-first")]
        [InlineData("_field", "field")]
        [InlineData("__field", "field")]
        [InlineData("___field", "field")]
        [InlineData("m_field", "m-field")]
        [InlineData("m_Field", "m-field")]
        public void ToKebabCase(string input, string expected)
        {
            Assert.Equal(expected, input.ToKebabCase());
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("NoEdit", "NO_EDIT")]
        [InlineData("word", "WORD")]
        [InlineData("_field", "FIELD")]
        [InlineData("MSBuildTask", "MSBUILD_TASK")]
        public void ToConstantCase(string input, string expected)
        {
            Assert.Equal(expected, input.ToConstantCase());
        }
    }
}