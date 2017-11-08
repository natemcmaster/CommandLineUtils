// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class HelpOptionTests
    {
        private class NoHelpOptionClass
        {
            [Option]
            public int OptionA { get; set; }
        }

        [Fact]
        public void DoesNotAddHelpOptionByDefault()
        {
            var builder = new ReflectionAppBuilder<NoHelpOptionClass>();
            Assert.Null(builder.App.OptionHelp);
        }
        
        private class MultipleHelpOptions
        {
            [HelpOption("-h1")]
            public bool IsHelp1 { get; set; }
            
            [HelpOption("-h2")]
            public bool IsHelp2 { get; set; }
        }

        [Fact]
        public void ThrowsWhenMultipleHelpOptionsInType()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new ReflectionAppBuilder<MultipleHelpOptions>());
            Assert.Equal(Strings.MultipleHelpOptionPropertiesFound, ex.Message);
        }
        
        [HelpOption]
        private class HelpOptionOnTypeAndProp
        {
            [HelpOption]
            public bool IsHelp { get; set; }
        }

        [Fact]
        public void ThrowsWhenMultipleHelpOptionUsedOnTypeAndProperti()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new ReflectionAppBuilder<HelpOptionOnTypeAndProp>());
            Assert.Equal(Strings.HelpOptionOnTypeAndProperty, ex.Message);
        }

        private class HelpOptionOnNonBoolean
        {
            [HelpOption]
            public string IsHelpOption { get; set; }
        }

        [Fact]
        public void ThrowsIfHelpOptionPropIsNotBool()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new ReflectionAppBuilder<HelpOptionOnNonBoolean>());
            Assert.Equal(Strings.NoValueTypesMustBeBoolean, ex.Message);
        }
        
        private class DuplicateOptionAttributes
        {
            [HelpOption]
            [Option]
            public string IsHelpOption { get; set; }
        }
        
        [Fact]
        public void ThrowsIfMultipleAttributesApplied()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new ReflectionAppBuilder<DuplicateOptionAttributes>());
            var prop = typeof(DuplicateOptionAttributes).GetProperty(nameof(DuplicateOptionAttributes.IsHelpOption));
            Assert.Equal(Strings.BothOptionAndHelpOptionAttributesCannotBeSpecified(prop), ex.Message);
        }

        [HelpOption("-h|--help", Description = "My help info")]
        private class WithTypeHelpOption
        {
        }

        [Fact]
        public void SetsHelpOptionOnType()
        {
            var builder = new ReflectionAppBuilder<WithTypeHelpOption>();
            Assert.NotNull(builder.App.OptionHelp);
            Assert.Equal(CommandOptionType.NoValue, builder.App.OptionHelp.OptionType);
            Assert.Null(builder.App.OptionHelp.SymbolName);
            Assert.Equal("h", builder.App.OptionHelp.ShortName);
            Assert.Equal("help", builder.App.OptionHelp.LongName);
            Assert.Equal("My help info", builder.App.OptionHelp.Description);
        }
        
        private class WithPropHelpOption
        {
            [HelpOption("-h|--help", Description = "My help info")]
            public bool IsHelp { get; }
        }

        [Fact]
        public void SetsHelpOptionOnProp()
        {
            var builder = new ReflectionAppBuilder<WithPropHelpOption>();
            Assert.NotNull(builder.App.OptionHelp);
            Assert.Equal(CommandOptionType.NoValue, builder.App.OptionHelp.OptionType);
            Assert.Null(builder.App.OptionHelp.SymbolName);
            Assert.Equal("h", builder.App.OptionHelp.ShortName);
            Assert.Equal("help", builder.App.OptionHelp.LongName);
            Assert.Equal("My help info", builder.App.OptionHelp.Description);
        }
    }
}
