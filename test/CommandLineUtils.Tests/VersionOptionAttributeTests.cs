// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class VersionOptionTests
    {
        private readonly ITestOutputHelper _output;

        public VersionOptionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private class NoVersionOptionClass
        {
            [Option]
            public int OptionA { get; set; }
        }

        [Fact]
        public void DoesNotAddVersionOptionByDefault()
        {
            var app = new CommandLineApplication<NoVersionOptionClass>();
            app.Conventions.UseVersionOptionAttribute();
            Assert.Null(app.OptionVersion);
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
                new CommandLineApplication<MultipleVersionOptions>().Conventions.UseVersionOptionAttribute());
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
                new CommandLineApplication<VersionOptionOnTypeAndProp>().Conventions.UseVersionOptionAttribute());
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
                new CommandLineApplication<VersionOptionOnNonBoolean>().Conventions.UseVersionOptionAttribute());
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
                new CommandLineApplication<DuplicateOptionAttributes>().Conventions.UseVersionOptionAttribute());
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
                new CommandLineApplication<DuplicateOptionAttributes2>().Conventions.UseVersionOptionAttribute());
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
            var app = new CommandLineApplication<WithTypeVersionOption>();
            app.Conventions.UseVersionOptionAttribute();
            Assert.NotNull(app.OptionVersion);
            Assert.Equal(CommandOptionType.NoValue, app.OptionVersion.OptionType);
            Assert.Null(app.OptionVersion.SymbolName);
            Assert.Null(app.OptionVersion.ShortName);
            Assert.Equal("version", app.OptionVersion.LongName);
            Assert.Equal("My version info", app.OptionVersion.Description);
        }

        private class WithPropVersionOption
        {
            [VersionOption("1.2.0", Description = "My version info")]
            public bool IsVersion { get; }
        }

        [Fact]
        public void SetsVersionOptionOnProp()
        {
            var app = new CommandLineApplication<WithPropVersionOption>();
            app.Conventions.UseVersionOptionAttribute();
            Assert.NotNull(app.OptionVersion);
            Assert.Equal(CommandOptionType.NoValue, app.OptionVersion.OptionType);
            Assert.Null(app.OptionVersion.SymbolName);
            Assert.Null(app.OptionVersion.ShortName);
            Assert.Equal("version", app.OptionVersion.LongName);
            Assert.Equal("My version info", app.OptionVersion.Description);
        }

        [VersionOption("-?|-V|--version", "1.0.0")]
        private class SimpleVersionApp
        {
            private void OnExecute()
            {
                throw new InvalidOperationException("This method should not be invoked");
            }
        }

        [Theory]
        [InlineData("-?")]
        [InlineData("-V")]
        [InlineData("--version")]
        public void OnExecuteIsNotInvokedWhenVersionOptionSpecified(string arg)
        {
            Assert.Equal(0, CommandLineApplication.Execute<SimpleVersionApp>(new TestConsole(_output), arg));
        }
    }
}
