// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class HelpOptionMetadataTests
    {
        [Fact]
        public void DefaultValues_AreCorrect()
        {
            var metadata = new HelpOptionMetadata();

            Assert.Null(metadata.Template);
            Assert.Null(metadata.Description);
            Assert.False(metadata.Inherited);
        }

        [Fact]
        public void Template_CanBeSet()
        {
            var metadata = new HelpOptionMetadata
            {
                Template = "-h|--help"
            };

            Assert.Equal("-h|--help", metadata.Template);
        }

        [Fact]
        public void Description_CanBeSet()
        {
            var metadata = new HelpOptionMetadata
            {
                Description = "Show help information"
            };

            Assert.Equal("Show help information", metadata.Description);
        }

        [Fact]
        public void Inherited_CanBeSet()
        {
            var metadata = new HelpOptionMetadata
            {
                Inherited = true
            };

            Assert.True(metadata.Inherited);
        }

        [Fact]
        public void AllProperties_CanBeSetTogether()
        {
            var metadata = new HelpOptionMetadata
            {
                Template = "-?|-h|--help",
                Description = "Display help for this command",
                Inherited = true
            };

            Assert.Equal("-?|-h|--help", metadata.Template);
            Assert.Equal("Display help for this command", metadata.Description);
            Assert.True(metadata.Inherited);
        }

        [Fact]
        public void DifferentInstances_AreSeparate()
        {
            var metadata1 = new HelpOptionMetadata
            {
                Template = "-h",
                Description = "Help 1",
                Inherited = false
            };

            var metadata2 = new HelpOptionMetadata
            {
                Template = "--help",
                Description = "Help 2",
                Inherited = true
            };

            Assert.NotEqual(metadata1.Template, metadata2.Template);
            Assert.NotEqual(metadata1.Description, metadata2.Description);
            Assert.NotEqual(metadata1.Inherited, metadata2.Inherited);
        }
    }
}
