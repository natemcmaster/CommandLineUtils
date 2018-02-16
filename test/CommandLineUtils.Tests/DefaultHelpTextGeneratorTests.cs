// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using McMaster.Extensions.CommandLineUtils.HelpText;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class DefaultHelpTextGeneratorTests
    {
        private readonly ITestOutputHelper _output;

        public DefaultHelpTextGeneratorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class EmptyShortName
        {
            [Option(ShortName = "")]
            public string Option { get; }
        }

        [Fact]
        public void ItListOptions()
        {
            var builder = new ReflectionAppBuilder<EmptyShortName>();
            builder.Initialize();
            var sb = new StringBuilder();
            DefaultHelpTextGenerator.Singleton.Generate(builder.App, new StringWriter(sb));
            var helpText = sb.ToString();
            _output.WriteLine(helpText);

            Assert.Contains("--option <OPTION>", helpText);
            Assert.DoesNotContain("-|--option <OPTION>", helpText);
        }
    }
}
