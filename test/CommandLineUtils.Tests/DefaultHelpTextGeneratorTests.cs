// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            var app = new CommandLineApplication<EmptyShortName>();
            app.Conventions.UseDefaultConventions();
            var sb = new StringBuilder();
            DefaultHelpTextGenerator.Singleton.Generate(app, new StringWriter(sb));
            var helpText = sb.ToString();
            _output.WriteLine(helpText);

            Assert.Contains("--option <OPTION>", helpText);
            Assert.DoesNotContain("-|--option <OPTION>", helpText);
        }

        [Fact]
        public void OrderCommandsByName()
        {
            var app = new CommandLineApplication<EmptyShortName>();
            app.Conventions.UseDefaultConventions();
            app.Command("b", null);
            app.Command("a", null);
            var sb = new StringBuilder();
            DefaultHelpTextGenerator.Singleton.Generate(app, new StringWriter(sb));
            var helpText = sb.ToString();
            _output.WriteLine(helpText);

            int indexOfA = helpText.IndexOf("  a", StringComparison.InvariantCulture);
            int indexOfB = helpText.IndexOf("  b", StringComparison.InvariantCulture);
            Assert.True(indexOfA < indexOfB);
        }

        [Fact]
        public void DontOrderCommandsByName()
        {
            DefaultHelpTextGenerator.Singleton.SortCommandsByName = false;
            var app = new CommandLineApplication<EmptyShortName>();
            app.Conventions.UseDefaultConventions();
            app.Command("b", null);
            app.Command("a", null);
            var sb = new StringBuilder();
            DefaultHelpTextGenerator.Singleton.Generate(app, new StringWriter(sb));
            var helpText = sb.ToString();
            _output.WriteLine(helpText);

            int indexOfA = helpText.IndexOf("  a", StringComparison.InvariantCulture);
            int indexOfB = helpText.IndexOf("  b", StringComparison.InvariantCulture);
            Assert.True(indexOfA > indexOfB);
        }
    }
}
