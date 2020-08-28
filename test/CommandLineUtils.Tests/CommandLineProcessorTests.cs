// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandLineProcessorTests
    {
        private readonly ITestOutputHelper _output;

        public CommandLineProcessorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(new[] { "-a" }, true, false, false, null)]
        [InlineData(new[] { "-aa" }, true, false, false, null)]
        [InlineData(new[] { "-aaaaa" }, true, false, false, null)]
        [InlineData(new[] { "-ab" }, true, true, false, null)]
        [InlineData(new[] { "-abc" }, true, true, true, null)]
        [InlineData(new[] { "-ca" }, true, false, true, null)]
        [InlineData(new[] { "-fignore-whitespace" }, false, false, false, "ignore-whitespace")]
        [InlineData(new[] { "-fPath.txt" }, false, false, false, "Path.txt")]
        [InlineData(new[] { "-afPath.txt" }, true, false, false, "Path.txt")]
        [InlineData(new[] { "-af:Path.txt" }, true, false, false, "Path.txt")]
        [InlineData(new[] { "-af=Path.txt" }, true, false, false, "Path.txt")]
        [InlineData(new[] { "-a", "-f=Path.txt" }, true, false, false, "Path.txt")]
        [InlineData(new[] { "-a", "-f:Path.txt" }, true, false, false, "Path.txt")]
        [InlineData(new[] { "-a", "-fPath.txt" }, true, false, false, "Path.txt")]
        [InlineData(new[] { "-af", "Path.txt" }, true, false, false, "Path.txt")]
        [InlineData(new[] { "-f:Path.txt" }, false, false, false, "Path.txt")]
        [InlineData(new[] { "-a", "-c", "-f", "Path.txt" }, true, false, true, "Path.txt")]
        public void CanParseClusteredOptions(string[] input, bool a, bool b, bool c, string f)
        {
            var app = new CommandLineApplication
            {
                ClusterOptions = true
            };

            var optA = app.Option("-a", "Option A", CommandOptionType.NoValue);
            var optB = app.Option("-b", "Option B", CommandOptionType.NoValue);
            var optC = app.Option("-c", "Option C", CommandOptionType.NoValue);
            var optF = app.Option("-f", "Option F", CommandOptionType.SingleValue);
            app.Parse(input);
            Assert.Equal(a, optA.HasValue());
            Assert.Equal(b, optB.HasValue());
            Assert.Equal(c, optC.HasValue());
            Assert.Equal(f, optF.Value());
        }

        [Fact]
        public void CanParseClusteredOptionMultipleTimes()
        {
            var app = new CommandLineApplication
            {
                ClusterOptions = true
            };

            var optA = app.Option("-a|--all", "Option A", CommandOptionType.NoValue);
            app.Parse("-aaa", "-a", "--all");
            Assert.True(optA.HasValue());
            Assert.Equal(5, optA.Values.Count);
        }

        [Theory]
        [InlineData("-vl:diag", "diag")]
        [InlineData("-vl=diag", "diag")]
        [InlineData("-vl", null)]
        [InlineData("-lv", null)]
        public void ItClustersSingleOrNoValueOptions(string input, string expectedLogValue)
        {
            var app = new CommandLineApplication
            {
                ClusterOptions = true
            };
            var verbose = app.Option("-v|--verbose", "Verbose output", CommandOptionType.NoValue);
            var log = app.Option("-l|--log[:<LEVEL>]", "Log level", CommandOptionType.SingleOrNoValue);

            app.Parse(input);

            Assert.True(verbose.HasValue());
            Assert.Single(verbose.Values);

            Assert.True(log.HasValue());
            Assert.Equal(log.Value(), expectedLogValue);
            Assert.Single(log.Values);
        }

        [Theory]
        [InlineData("-vd", "-d")]
        [InlineData("-vdiag", "-d")]
        [InlineData("-d", "-d")]
        public void ItThrowsForUnrecognizedClusterOption(string input, string unrecognizedOption)
        {
            var app = new CommandLineApplication
            {
                ClusterOptions = true
            };

            app.Option("-v|--verbose", "Verbose output", CommandOptionType.NoValue);

            var ex = Assert.ThrowsAny<CommandParsingException>(() => app.Parse(input));
            Assert.Equal($"Unrecognized option '{unrecognizedOption}'", ex.Message);
        }

        [Fact]
        public void ItThrowsIfClusteringIsSetButOptionShortNameIsTooLong()
        {
            var app = new CommandLineApplication
            {
                ClusterOptions = true
            };

            app.Option("-au|--auth", "Verbose output", CommandOptionType.NoValue);

            Assert.Throws<CommandParsingException>(() => app.Parse());
        }

        [Command]
        private class ShortNameType
        {
            [Option(ShortName = "au")]
            public string? Auth { get; }
        }

        [Command]
        [Subcommand(typeof(ShortNameType))]
        private class ParentCommand
        {

        }

        [Command]
        [Subcommand(typeof(ParentCommand))]
        private class ParentParentCommand
        {
        }

        [Fact]
        public void ItInfersClusterOptionsCannotBeUsed_MultiCharShortName()
        {
            var app = new CommandLineApplication();
            app.Option("-au|--auth", "Verbose output", CommandOptionType.NoValue);
            app.Parse();
            Assert.False(app.ClusterOptions);
        }

        [Fact]
        public void ItInfersClusterOptionsCannotBeUsed_MultiCharShortNameInSubcommand()
        {
            var app = new CommandLineApplication();
            app.Command("sub", c => c.Option("-au|--auth", "Verbose output", CommandOptionType.NoValue));
            Assert.False(app.ClusterOptionsWasSetExplicitly);
            app.Parse("sub", "-au");
            Assert.False(app.ClusterOptions);
        }

        [Fact]
        public void ItInfersClusterOptionsCannotBeUsed_MultiCharShortNameAttribute()
        {
            var app = new CommandLineApplication<ShortNameType>();
            app.Conventions.UseDefaultConventions();
            Assert.False(app.ClusterOptionsWasSetExplicitly);
            app.Parse();
            Assert.False(app.ClusterOptions);
        }

        [Fact]
        public void ItInfersClusterOptionsCannotBeUsed_MultiCharShortNameAttributeSubcommand()
        {
            var app = new CommandLineApplication<ParentCommand>();
            app.Conventions.UseDefaultConventions();
            Assert.False(app.ClusterOptionsWasSetExplicitly);
            app.Parse();
            Assert.False(app.ClusterOptions);
        }

        [Fact]
        public void ItInfersClusterOptionsCannotBeUsed()
        {
            var app = new CommandLineApplication<ParentParentCommand>();
            app.Conventions.UseDefaultConventions();
            Assert.False(app.ClusterOptionsWasSetExplicitly);
            app.Parse();
            Assert.False(app.ClusterOptions);
        }

        [Fact]
        public void ItDefaultsToClusterOptions()
        {
            var app = new CommandLineApplication();
            app.Option("-a|--auth", "Verbose output", CommandOptionType.NoValue);
            app.Parse();
            Assert.True(app.ClusterOptions);
        }

        [Fact]
        public void ClusterOptionsSettingIsInherited()
        {
            var app = new CommandLineApplication
            {
                ClusterOptions = false
            };
            var cmd = app.Command("save", c => { });
            Assert.Equal(app.ClusterOptions, cmd.ClusterOptions);
        }

        [Theory]
        [InlineData("-lv:value", 'l')]
        [InlineData("-lv:", 'l')]
        [InlineData("-lv=", 'l')]
        [InlineData("-fv=", 'f')]
        [InlineData("-fv:", 'f')]
        [InlineData("-fv=path", 'f')]
        public void ItThrowsForAmbiguousValueInClusters(string input, char badOption)
        {
            var app = new CommandLineApplication
            {
                ClusterOptions = true
            };
            app.Option("-v|--verbose", "Verbose output", CommandOptionType.NoValue);
            app.Option("-l|--log:<LEVEL>", "Log level", CommandOptionType.SingleValue);
            app.Option("-f:<File>", "Files", CommandOptionType.MultipleValue);

            var ex = Assert.Throws<CommandParsingException>(() => app.Parse(input));
            Assert.Equal($"Option '{badOption}', which requires a value, must be the last option in a cluster",
                ex.Message);
        }

        [Fact]
        public void CanUseSingleDashAsArgumentValue()
        {
            var app = new CommandLineApplication();
            var arg = app.Argument("Input", "Input");
            app.Parse("-");
            Assert.Equal("-", arg.Value);
        }

        [Theory]
        [InlineData("--log", null)]
        [InlineData("--log:", "")]
        [InlineData("--log: ", " ")]
        [InlineData("--log:verbose", "verbose")]
        [InlineData("--log=verbose", "verbose")]
        public void CanParseSingleOrNoValueParameter(string input, string expected)
        {
            var app = new CommandLineApplication();
            var opt = app.Option("--log", "Log level", CommandOptionType.SingleOrNoValue);
            app.Parse(input);
            Assert.True(opt.HasValue(), "Option should have value");
            Assert.Equal(expected, opt.Value());
            Assert.Empty(app.RemainingArguments);
        }

        [Theory]
        [InlineData(new[] { "--param1" }, null, null)]
        [InlineData(new[] { "--param1", "--param2", "p2" }, null, "p2")]
        [InlineData(new[] { "--param1:p1", "--param2", "p2" }, "p1", "p2")]
        public void CanParseSingleOrNoValueParameters(string[] args, string param1, string param2)
        {
            var app = new CommandLineApplication();
            var opt1 = app.Option("--param1", "param1", CommandOptionType.SingleOrNoValue);
            var opt2 = app.Option("--param2", "param2", CommandOptionType.SingleValue);
            app.Parse(args);
            Assert.Equal(param1, opt1.Value());
            Assert.Equal(param2, opt2.Value());
            Assert.Empty(app.RemainingArguments);
        }

        [Fact]
        public void ThrowsWhenSingleValueIsNotProvided()
        {
            var app = new CommandLineApplication();
            app.Option("--log", "Log level", CommandOptionType.SingleValue);
            Assert.Throws<CommandParsingException>(() => app.Parse("--log"));
        }

        [Fact]
        public void ItFindsSubcommandsByAliases()
        {
            var app = new CommandLineApplication();
            var cmd = app.Command("sub1", c => { c.AddName("s"); });

            var result = app.Parse("s");
            Assert.Same(cmd, result.SelectedCommand);
        }
    }
}
