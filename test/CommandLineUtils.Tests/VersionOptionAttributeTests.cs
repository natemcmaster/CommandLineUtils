// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class VersionOptionTests
    {
        private class NoVersionOptionClass
        {
            [Option]
            public int OptionA { get; set; }
        }

        [Fact]
        public void DoesNotAddVersionOptionByDefault()
        {
            var builder = new ReflectionAppBuilder<NoVersionOptionClass>();
            builder.Initialize();
            Assert.Null(builder.App.OptionVersion);
        }

        private class MultipleVersionOptions
        {
            [VersionOption("-v1")]
            public bool IsVersion1 { get; set; }

            [VersionOption("-v2")]
            public bool IsVersion2 { get; set; }
        }

        [Fact]
        public void ThrowsWhenMultipleVersionOptionsInType()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new ReflectionAppBuilder<MultipleVersionOptions>().Initialize());
            Assert.Equal(Strings.MultipleVersionOptionPropertiesFound, ex.Message);
        }

        [VersionOption("1.2.0")]
        private class VersionOptionOnTypeAndProp
        {
            [VersionOption("1.2.0")]
            public bool IsVersion { get; set; }
        }

        [Fact]
        public void ThrowsWhenMultipleVersionOptionUsedOnTypeAndProperti()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new ReflectionAppBuilder<VersionOptionOnTypeAndProp>().Initialize());
            Assert.Equal(Strings.VersionOptionOnTypeAndProperty, ex.Message);
        }

        private class VersionOptionOnNonBoolean
        {
            [VersionOption("1.2.0")]
            public string IsVersionOption { get; set; }
        }

        [Fact]
        public void ThrowsIfVersionOptionPropIsNotBool()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new ReflectionAppBuilder<VersionOptionOnNonBoolean>().Initialize());
            Assert.Equal(Strings.NoValueTypesMustBeBoolean, ex.Message);
        }

        private class DuplicateOptionAttributes
        {
            [VersionOption("1.2.0")]
            [Option]
            public string IsVersionOption { get; set; }
        }

        [Fact]
        public void ThrowsIfMultipleAttributesApplied()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new ReflectionAppBuilder<DuplicateOptionAttributes>().Initialize());
            var prop = typeof(DuplicateOptionAttributes).GetProperty(nameof(DuplicateOptionAttributes.IsVersionOption));
            Assert.Equal(Strings.BothOptionAndVersionOptionAttributesCannotBeSpecified(prop), ex.Message);
        }

        private class DuplicateOptionAttributes2
        {
            [VersionOption("1.2.0")]
            [HelpOption]
            public string IsVersionOption { get; set; }
        }

        [Fact]
        public void ThrowsIfHelpAndVersionAttributesApplied()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new ReflectionAppBuilder<DuplicateOptionAttributes2>().Initialize());
            var prop = typeof(DuplicateOptionAttributes2).GetProperty(nameof(DuplicateOptionAttributes
                .IsVersionOption));
            Assert.Equal(Strings.BothHelpOptionAndVersionOptionAttributesCannotBeSpecified(prop), ex.Message);
        }

        [VersionOption("1.2.0", Description = "My version info")]
        private class WithTypeVersionOption
        {
        }

        [Fact]
        public void SetsVersionOptionOnType()
        {
            var builder = new ReflectionAppBuilder<WithTypeVersionOption>();
            builder.Initialize();
            Assert.NotNull(builder.App.OptionVersion);
            Assert.Equal(CommandOptionType.NoValue, builder.App.OptionVersion.OptionType);
            Assert.Null(builder.App.OptionVersion.SymbolName);
            Assert.Null(builder.App.OptionVersion.ShortName);
            Assert.Equal("version", builder.App.OptionVersion.LongName);
            Assert.Equal("My version info", builder.App.OptionVersion.Description);
        }

        private class WithPropVersionOption
        {
            [VersionOption("1.2.0", Description = "My version info")]
            public bool IsVersion { get; }
        }

        [Fact]
        public void SetsVersionOptionOnProp()
        {
            var builder = new ReflectionAppBuilder<WithPropVersionOption>();
            builder.Initialize();
            Assert.NotNull(builder.App.OptionVersion);
            Assert.Equal(CommandOptionType.NoValue, builder.App.OptionVersion.OptionType);
            Assert.Null(builder.App.OptionVersion.SymbolName);
            Assert.Null(builder.App.OptionVersion.ShortName);
            Assert.Equal("version", builder.App.OptionVersion.LongName);
            Assert.Equal("My version info", builder.App.OptionVersion.Description);
        }
    }
}
