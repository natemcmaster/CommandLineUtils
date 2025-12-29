// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class CommandMetadataTests
    {
        [Fact]
        public void DefaultValues_AreCorrect()
        {
            var metadata = new CommandMetadata();

            Assert.Null(metadata.Name);
            Assert.Null(metadata.AdditionalNames);
            Assert.Null(metadata.FullName);
            Assert.Null(metadata.Description);
            Assert.Null(metadata.ExtendedHelpText);
            Assert.True(metadata.ShowInHelpText);
            Assert.Null(metadata.AllowArgumentSeparator);
            Assert.Null(metadata.ClusterOptions);
            Assert.Null(metadata.OptionsComparison);
            Assert.Null(metadata.ParseCulture);
            Assert.Null(metadata.ResponseFileHandling);
            Assert.Null(metadata.UnrecognizedArgumentHandling);
            Assert.Null(metadata.UsePagerForHelpText);
        }

        [Fact]
        public void AllProperties_CanBeSet()
        {
            var metadata = new CommandMetadata
            {
                Name = "myapp",
                AdditionalNames = new[] { "alias1", "alias2" },
                FullName = "My Application",
                Description = "A test application",
                ExtendedHelpText = "Extended help here",
                ShowInHelpText = false,
                AllowArgumentSeparator = true,
                ClusterOptions = false,
                OptionsComparison = StringComparison.OrdinalIgnoreCase,
                ParseCulture = System.Globalization.CultureInfo.InvariantCulture,
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
                UsePagerForHelpText = true
            };

            Assert.Equal("myapp", metadata.Name);
            Assert.Equal(new[] { "alias1", "alias2" }, metadata.AdditionalNames);
            Assert.Equal("My Application", metadata.FullName);
            Assert.Equal("A test application", metadata.Description);
            Assert.Equal("Extended help here", metadata.ExtendedHelpText);
            Assert.False(metadata.ShowInHelpText);
            Assert.True(metadata.AllowArgumentSeparator);
            Assert.False(metadata.ClusterOptions);
            Assert.Equal(StringComparison.OrdinalIgnoreCase, metadata.OptionsComparison);
            Assert.Equal(System.Globalization.CultureInfo.InvariantCulture, metadata.ParseCulture);
            Assert.Equal(ResponseFileHandling.ParseArgsAsLineSeparated, metadata.ResponseFileHandling);
            Assert.Equal(UnrecognizedArgumentHandling.StopParsingAndCollect, metadata.UnrecognizedArgumentHandling);
            Assert.True(metadata.UsePagerForHelpText);
        }

        [Fact]
        public void ApplyTo_SetsAppName()
        {
            var metadata = new CommandMetadata { Name = "testapp" };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.Equal("testapp", app.Name);
        }

        [Fact]
        public void ApplyTo_SetsDescription()
        {
            var metadata = new CommandMetadata { Description = "Test description" };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.Equal("Test description", app.Description);
        }

        [Fact]
        public void ApplyTo_SetsFullName()
        {
            var metadata = new CommandMetadata { FullName = "Full Name Here" };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.Equal("Full Name Here", app.FullName);
        }

        [Fact]
        public void ApplyTo_SetsExtendedHelpText()
        {
            var metadata = new CommandMetadata { ExtendedHelpText = "Extended help text" };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.Equal("Extended help text", app.ExtendedHelpText);
        }

        [Fact]
        public void ApplyTo_SetsAllowArgumentSeparator()
        {
            var metadata = new CommandMetadata { AllowArgumentSeparator = true };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.True(app.AllowArgumentSeparator);
        }

        [Fact]
        public void ApplyTo_SetsClusterOptions_WhenSpecified()
        {
            var metadata = new CommandMetadata { ClusterOptions = false };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.False(app.ClusterOptions);
        }

        [Fact]
        public void ApplyTo_DoesNotSetClusterOptions_WhenNull()
        {
            var metadata = new CommandMetadata { ClusterOptions = null };
            var app = new CommandLineApplication { ClusterOptions = true };

            metadata.ApplyTo(app);

            // Should retain original value
            Assert.True(app.ClusterOptions);
        }

        [Fact]
        public void ApplyTo_SetsUnrecognizedArgumentHandling_WhenSpecified()
        {
            var metadata = new CommandMetadata
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue
            };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.Equal(UnrecognizedArgumentHandling.CollectAndContinue, app.UnrecognizedArgumentHandling);
        }

        [Fact]
        public void ApplyTo_DoesNotSetUnrecognizedArgumentHandling_WhenNull()
        {
            var metadata = new CommandMetadata { UnrecognizedArgumentHandling = null };
            var app = new CommandLineApplication
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect
            };

            metadata.ApplyTo(app);

            // Should retain original value
            Assert.Equal(UnrecognizedArgumentHandling.StopParsingAndCollect, app.UnrecognizedArgumentHandling);
        }

        [Fact]
        public void ApplyTo_SetsUsePagerForHelpText()
        {
            var metadata = new CommandMetadata { UsePagerForHelpText = true };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.True(app.UsePagerForHelpText);
        }

        [Fact]
        public void ApplyTo_SetsOptionsComparison_WhenSpecified()
        {
            var metadata = new CommandMetadata
            {
                OptionsComparison = StringComparison.OrdinalIgnoreCase
            };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.Equal(StringComparison.OrdinalIgnoreCase, app.OptionsComparison);
        }

        [Fact]
        public void ApplyTo_SetsResponseFileHandling_WhenSpecified()
        {
            var metadata = new CommandMetadata
            {
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated
            };
            var app = new CommandLineApplication();

            metadata.ApplyTo(app);

            Assert.Equal(ResponseFileHandling.ParseArgsAsLineSeparated, app.ResponseFileHandling);
        }

        [Fact]
        public void ApplyTo_DoesNotOverwriteWithNull_Values()
        {
            var metadata = new CommandMetadata(); // All defaults/nulls
            var app = new CommandLineApplication
            {
                Name = "original",
                Description = "original desc",
                FullName = "Original Full",
                ExtendedHelpText = "Original Extended"
            };

            metadata.ApplyTo(app);

            // When metadata has null values, existing app values should be preserved
            Assert.Equal("original", app.Name);
            Assert.Equal("original desc", app.Description);
            Assert.Equal("Original Full", app.FullName);
            Assert.Equal("Original Extended", app.ExtendedHelpText);
        }
    }
}
