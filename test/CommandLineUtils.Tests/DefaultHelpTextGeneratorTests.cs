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
            public string? Option { get; }
        }

        [Fact]
        public void ItFormatsOptions()
        {
            var app = new CommandLineApplication();
            var option = app.Option("-a|--all <ALL>", "All", CommandOptionType.SingleValue);
            option.ShortName = "b";
            var helpText = GetHelpText(app);

            Assert.Contains("-b|--all <ALL>", helpText);
            Assert.DoesNotContain("-a|--all <ALL>", helpText);
        }

        [Fact]
        public void ItListOptions()
        {
            var app = new CommandLineApplication<EmptyShortName>();
            app.Conventions.UseDefaultConventions();
            var helpText = GetHelpText(app);

            Assert.Contains("--option <OPTION>", helpText);
            Assert.DoesNotContain("-|--option <OPTION>", helpText);
        }

        [Fact]
        public void OrderCommandsByName()
        {
            var app = new CommandLineApplication<EmptyShortName>();
            app.Conventions.UseDefaultConventions();
            app.Command("b", _ => { });
            app.Command("a", _ => { });
            var helpText = GetHelpText(app);

            var indexOfA = helpText.IndexOf("  a", StringComparison.InvariantCulture);
            var indexOfB = helpText.IndexOf("  b", StringComparison.InvariantCulture);
            Assert.True(indexOfA < indexOfB);
        }

        [Fact]
        public void DoesNotOrderCommandsByName()
        {
            DefaultHelpTextGenerator.Singleton.SortCommandsByName = false;
            var app = new CommandLineApplication<EmptyShortName>();
            app.Conventions.UseDefaultConventions();
            app.Command("b", _ => { });
            app.Command("a", _ => { });
            var helpText = GetHelpText(app);

            var indexOfA = helpText.IndexOf("  a", StringComparison.InvariantCulture);
            var indexOfB = helpText.IndexOf("  b", StringComparison.InvariantCulture);
            Assert.True(indexOfA > indexOfB);
        }

        private string GetHelpText(CommandLineApplication app)
        {
            var sb = new StringBuilder();
            DefaultHelpTextGenerator.Singleton.Generate(app, new StringWriter(sb));
            var helpText = sb.ToString();
            _output.WriteLine(helpText);
            return helpText;
        }
        
        enum SomeEnum { None, Normal, Extreme }

        [Fact]
        public void ShowHelp()
        {
            var app = new CommandLineApplication();
            app.HelpOption();
            app.Option("--strOpt <E>", "int option desc", CommandOptionType.SingleValue);
            app.Option<int>("--intOpt <E>", "int option desc", CommandOptionType.SingleValue);
            app.Option<SomeEnum>("--enumOpt <E>", "enum option desc", CommandOptionType.SingleValue);
            app.Argument("SomeArgument1", "arg desc");
            app.Argument<SomeEnum>("SomeArgument2", "arg desc");

            StringWriter sw = new StringWriter();
            app.Out = sw;
            app.ShowHelp(false);

            Assert.Equal(@"Usage:  [options] <SomeArgument1> <SomeArgument2>

Arguments:
  SomeArgument1  arg desc
  SomeArgument2  arg desc
                 Allowed values are: None, Normal, Extreme

Options:
  -?|-h|--help   Show help information
  --strOpt <E>   int option desc
  --intOpt <E>   int option desc
  --enumOpt <E>  enum option desc
                 Allowed values are: None, Normal, Extreme

",
            sw.ToString(),
            ignoreLineEndingDifferences: true);
        }
    }
}
