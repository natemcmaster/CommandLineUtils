// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class VersionOptionMetadataTests
    {
        private class TestModel
        {
            public string? AppVersion { get; set; }
        }

        [Fact]
        public void DefaultValues_AreCorrect()
        {
            var metadata = new VersionOptionMetadata();

            Assert.Null(metadata.Template);
            Assert.Null(metadata.Description);
            Assert.Null(metadata.Version);
            Assert.Null(metadata.VersionGetter);
        }

        [Fact]
        public void Template_CanBeSet()
        {
            var metadata = new VersionOptionMetadata
            {
                Template = "-v|--version"
            };

            Assert.Equal("-v|--version", metadata.Template);
        }

        [Fact]
        public void Description_CanBeSet()
        {
            var metadata = new VersionOptionMetadata
            {
                Description = "Show version information"
            };

            Assert.Equal("Show version information", metadata.Description);
        }

        [Fact]
        public void Version_CanBeSet()
        {
            var metadata = new VersionOptionMetadata
            {
                Version = "1.0.0"
            };

            Assert.Equal("1.0.0", metadata.Version);
        }

        [Fact]
        public void VersionGetter_CanBeSetAndInvoked()
        {
            var metadata = new VersionOptionMetadata
            {
                VersionGetter = obj => ((TestModel)obj).AppVersion
            };

            var model = new TestModel { AppVersion = "2.0.0-beta" };

            var version = metadata.VersionGetter!(model);

            Assert.Equal("2.0.0-beta", version);
        }

        [Fact]
        public void AllProperties_CanBeSetTogether()
        {
            var metadata = new VersionOptionMetadata
            {
                Template = "--ver|--version",
                Description = "Display application version",
                Version = "3.1.0",
                VersionGetter = obj => ((TestModel)obj).AppVersion
            };

            Assert.Equal("--ver|--version", metadata.Template);
            Assert.Equal("Display application version", metadata.Description);
            Assert.Equal("3.1.0", metadata.Version);
            Assert.NotNull(metadata.VersionGetter);
        }

        [Fact]
        public void VersionGetter_ReturnsNull_WhenPropertyIsNull()
        {
            var metadata = new VersionOptionMetadata
            {
                VersionGetter = obj => ((TestModel)obj).AppVersion
            };

            var model = new TestModel { AppVersion = null };

            var version = metadata.VersionGetter!(model);

            Assert.Null(version);
        }

        [Fact]
        public void DifferentInstances_AreSeparate()
        {
            var metadata1 = new VersionOptionMetadata
            {
                Version = "1.0.0"
            };

            var metadata2 = new VersionOptionMetadata
            {
                Version = "2.0.0"
            };

            Assert.NotEqual(metadata1.Version, metadata2.Version);
        }
    }
}
