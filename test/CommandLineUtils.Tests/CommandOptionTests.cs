// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandOptionTests
    {
        [Theory]
        [InlineData("-a", "a", null, null, null)]
        [InlineData("-abc", "abc", null, null, null)]
        [InlineData("-a|--name", "a", null, "name", null)]
        [InlineData("-a|--name <VALUE>", "a", null, "name", "VALUE")]
        [InlineData("-a|--name <VALUE> <VALUE2>", "a", null, "name", "VALUE2")]
        [InlineData("-a <VALUE>", "a", null, null, "VALUE")]
        [InlineData("-?|-a", "a", "?", null, null)]
        [InlineData("-?|-a|--name", "a", "?", "name", null)]
        [InlineData("-?|-a|--name <VALUE>", "a", "?", "name", "VALUE")]
        [InlineData("-?|-a <VALUE>", "a", "?", null, "VALUE")]
        [InlineData("-? <VALUE>", null, "?", null, "VALUE")]
        [InlineData("-a --name", "a", null, "name", null)]
        [InlineData("-a --name <VALUE>", "a", null, "name", "VALUE")]
        [InlineData("-a -? --name <VALUE>", "a", "?", "name", "VALUE")]
        [InlineData("--name:<VALUE>", null, null, "name", "VALUE")]
        [InlineData("--name=<VALUE>", null, null, "name", "VALUE")]
        [InlineData("-a:<VALUE> --name:<VALUE>", "a", null, "name", "VALUE")]
        [InlineData("-a=<VALUE> --name=<VALUE>", "a", null, "name", "VALUE")]
        public void ItParsesSingleValueTemplate(string template, string shortName, string symbolName, string longName, string valueName)
        {
            var opt = new CommandOption(template, CommandOptionType.SingleValue);
            Assert.Equal(shortName, opt.ShortName);
            Assert.Equal(symbolName, opt.SymbolName);
            Assert.Equal(longName, opt.LongName);
            Assert.Equal(valueName, opt.ValueName);
        }

        [Theory]
        [InlineData("--name[:<VALUE>]", null, null, "name", "VALUE")]
        [InlineData("--name[=<VALUE>]", null, null, "name", "VALUE")]
        public void ItParsesSingleOrNoValueTemplate(string template, string shortName, string symbolName, string longName, string valueName)
        {
            var opt = new CommandOption(template, CommandOptionType.SingleOrNoValue);
            Assert.Equal(shortName, opt.ShortName);
            Assert.Equal(symbolName, opt.SymbolName);
            Assert.Equal(longName, opt.LongName);
            Assert.Equal(valueName, opt.ValueName);
        }
    }
}
