// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            var app = new CommandLineApplication<EmptyShortName>();
            app.Conventions.UseDefaultConventions();
            app.Command("b", _ => { });
            app.Command("a", _ => { });
            var generator = new DefaultHelpTextGenerator() { SortCommandsByName = false };
            var helpText = GetHelpText(app, generator);

            var indexOfA = helpText.IndexOf("  a", StringComparison.InvariantCulture);
            var indexOfB = helpText.IndexOf("  b", StringComparison.InvariantCulture);
            Assert.True(indexOfA > indexOfB);
        }

        private string GetHelpText(CommandLineApplication app, DefaultHelpTextGenerator generator)
        {
            var sb = new StringBuilder();
            generator.Generate(app, new StringWriter(sb));
            var helpText = sb.ToString();

            _output.WriteLine(helpText);

            return helpText;
        }

        private string GetHelpText(CommandLineApplication app)
        {
            var generator = new DefaultHelpTextGenerator
            {
                MaxLineLength = 80
            };

            return GetHelpText(app, generator);
        }

        public enum SomeEnum { None, Normal, Extreme }

        [Fact]
        public void ShowHelp()
        {
            var app = new CommandLineApplication();
            app.HelpOption();
            app.Option("--strOpt <E>", "str option desc.", CommandOptionType.SingleValue);
            app.Option("--rStrOpt <E>", "restricted str option desc.", CommandOptionType.SingleValue, o => o.IsRequired().Accepts().Values("Foo", "Bar"));
            app.Option<int>("--intOpt <E>", "int option desc.", CommandOptionType.SingleValue);
            app.Option<SomeEnum>("--enumOpt <E>", "enum option desc.", CommandOptionType.SingleValue);
            app.Option<SomeEnum>("--enumOpt2 <E>", "restricted enum option desc.", CommandOptionType.SingleValue, o => o.Accepts().Values("None", "Normal"));
            app.Option<(bool, SomeEnum)>("--enumOpt3 <E>", "nullable enum option desc.", CommandOptionType.SingleOrNoValue);
            app.Option<SomeEnum?>("--enumOpt4 <E>", "nullable enum option desc.", CommandOptionType.SingleOrNoValue);
            app.Argument("SomeStringArgument", "string arg desc.");
            app.Argument("RestrictedStringArgument", "restricted string arg desc.", a => a.IsRequired().Accepts().Values("Foo", "Bar"));
            app.Argument<SomeEnum>("SomeEnumArgument", "enum arg desc.");
            app.Argument<SomeEnum>("RestrictedEnumArgument", "restricted enum arg desc.", a => a.Accepts().Values("None", "Normal"));
            app.Argument<(bool, SomeEnum)>("SomeNullableEnumArgument", "nullable enum arg desc.");
            var helpText = GetHelpText(app);

            Assert.Equal(@"Usage:  [options] <SomeStringArgument> <RestrictedStringArgument> <SomeEnumArgument> <RestrictedEnumArgument> <SomeNullableEnumArgument>

Arguments:
  SomeStringArgument        string arg desc.
  RestrictedStringArgument  restricted string arg desc.
                            Allowed values are: Foo, Bar.
  SomeEnumArgument          enum arg desc.
                            Allowed values are: None, Normal, Extreme.
  RestrictedEnumArgument    restricted enum arg desc.
                            Allowed values are: None, Normal.
  SomeNullableEnumArgument  nullable enum arg desc.
                            Allowed values are: None, Normal, Extreme.

Options:
  -?|-h|--help              Show help information.
  --strOpt <E>              str option desc.
  --rStrOpt <E>             restricted str option desc.
                            Allowed values are: Foo, Bar.
  --intOpt <E>              int option desc.
  --enumOpt <E>             enum option desc.
                            Allowed values are: None, Normal, Extreme.
  --enumOpt2 <E>            restricted enum option desc.
                            Allowed values are: None, Normal.
  --enumOpt3[:<E>]          nullable enum option desc.
                            Allowed values are: None, Normal, Extreme.
  --enumOpt4[:<E>]          nullable enum option desc.
                            Allowed values are: None, Normal, Extreme.

",
            helpText,
            ignoreLineEndingDifferences: true);
        }


        [Fact]
        public void ShowHelpFromAttributes()
        {
            var app = new CommandLineApplication<MyApp>() { Name = "test" };
            app.Conventions.UseDefaultConventions();
            var helpText = GetHelpText(app);

            Assert.Equal(@"Usage: test [options] <SomeStringArgument> <RestrictedStringArgument> <SomeEnumArgument> <RestrictedEnumArgument> <SomeNullableEnumArgument>

Arguments:
  SomeStringArgument                string arg desc.
  RestrictedStringArgument          restricted string arg desc.
                                    Allowed values are: Foo, Bar.
  SomeEnumArgument                  enum arg desc.
                                    Allowed values are: None, Normal, Extreme.
  RestrictedEnumArgument            restricted enum arg desc.
                                    Allowed values are: None, Normal.
  SomeNullableEnumArgument          nullable enum arg desc.
                                    Allowed values are: None, Normal, Extreme.

Options:
  -strOpt|--str-opt <STR_OPT>       str option desc.
  -rStrOpt|--r-str-opt <R_STR_OPT>  restricted str option desc.
                                    Allowed values are: Foo, Bar.
  -intOpt|--int-opt <INT_OPT>       int option desc.
  -enumOpt|--verbosity <VERBOSITY>  enum option desc.
                                    Allowed values are: None, Normal, Extreme.
  -enumOpt2|--verb2 <VERB2>         restricted enum option desc.
                                    Allowed values are: None, Normal.
  -enumOpt3|--verb3[:<VERB3>]       nullable enum option desc.
                                    Allowed values are: None, Normal, Extreme.
  -enumOpt4|--verb4[:<VERB4>]       nullable enum option desc.
                                    Allowed values are: None, Normal, Extreme.
  -?|-h|--help                      Show help information.

",
                helpText,
                ignoreLineEndingDifferences: true);
        }

        public class MyApp
        {
            [Option(ShortName = "strOpt", Description = "str option desc.")]
            public string strOpt { get; set; }

            [Option(ShortName = "rStrOpt", Description = "restricted str option desc.")]
            [Required]
            [AllowedValues("Foo", "Bar")]
            public string rStrOpt { get; set; }

            [Option(ShortName = "intOpt", Description = "int option desc.")]
            public int intOpt { get; set; }

            [Option(ShortName = "enumOpt", Description = "enum option desc.")]
            public SomeEnum Verbosity { get; set; }

            [Option(ShortName = "enumOpt2", Description = "restricted enum option desc.")]
            [AllowedValues("None", "Normal")]
            public SomeEnum Verb2 { get; set; }

            [Option(ShortName = "enumOpt3", Description = "nullable enum option desc.")]
            public (bool HasValue, SomeEnum Value) Verb3 { get; set; }

            [Option(CommandOptionType.SingleOrNoValue, ShortName = "enumOpt4", Description = "nullable enum option desc.")]
            public SomeEnum? Verb4 { get; set; }

            [Argument(0, Description = "string arg desc.")]
            public string SomeStringArgument { get; set; }

            [Argument(1, Description = "restricted string arg desc.")]
            [Required]
            [AllowedValues("Foo", "Bar")]
            public string RestrictedStringArgument { get; set; }

            [Argument(2, Description = "enum arg desc.")]
            public SomeEnum SomeEnumArgument { get; set; }

            [Argument(3, Description = "restricted enum arg desc.")]
            [AllowedValues("None", "Normal")]
            public SomeEnum RestrictedEnumArgument { get; set; }

            [Argument(4, Description = "nullable enum arg desc.")]
            public (bool HasValue, SomeEnum Value) SomeNullableEnumArgument { get; set; }
        }

        [Theory]
        [InlineData("-h", "-h", "  -h          Show help information.", "  Subcommand ")]
        [InlineData("--help", "--help", "  --help      Show help information.", "  Subcommand ")]
        [InlineData("-?", "-?", "  -?          Show help information.", "  Subcommand ")]
        [InlineData(null, "-?|-h|--help", "  -?|-h|--help  Show help information.", "  Subcommand   ")]
        public void ShowHelpWithSubcommands(string helpOption, string expectedHintText, string expectedOptionsText,
            string expectedCommandsText)
        {
            var app = new CommandLineApplication { Name = "test" };
            if (helpOption != null) app.HelpOption(helpOption);
            app.Command("Subcommand", _ => { });
            app.Conventions.UseDefaultConventions();
            var helpText = GetHelpText(app);

            Assert.Equal($@"Usage: test [command] [options]

Options:
{expectedOptionsText}

Commands:
{expectedCommandsText} 

Run 'test [command] {expectedHintText}' for more information about a command.

",
                helpText,
                ignoreLineEndingDifferences: true);
        }
    }
}
