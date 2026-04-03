// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class SubcommandMetadataTests
    {
        [Command(Name = "test")]
        private class TestSubcommand
        {
            [Option("-n|--name")]
            public string? Name { get; set; }
        }

        [Command(Name = "other")]
        private class OtherSubcommand
        {
            [Option("-v|--verbose")]
            public bool Verbose { get; set; }
        }

        [Fact]
        public void Constructor_SetsSubcommandType()
        {
            var metadata = new SubcommandMetadata(typeof(TestSubcommand));

            Assert.Equal(typeof(TestSubcommand), metadata.SubcommandType);
        }

        [Fact]
        public void CommandName_DefaultsToNull()
        {
            var metadata = new SubcommandMetadata(typeof(TestSubcommand));

            Assert.Null(metadata.CommandName);
        }

        [Fact]
        public void CommandName_CanBeSet()
        {
            var metadata = new SubcommandMetadata(typeof(TestSubcommand))
            {
                CommandName = "test-cmd"
            };

            Assert.Equal("test-cmd", metadata.CommandName);
        }

        [Fact]
        public void MetadataProviderFactory_DefaultsToNull()
        {
            var metadata = new SubcommandMetadata(typeof(TestSubcommand));

            Assert.Null(metadata.MetadataProviderFactory);
        }

        [Fact]
        public void MetadataProviderFactory_CanBeSetAndInvoked()
        {
            var providerCreated = false;

            var metadata = new SubcommandMetadata(typeof(TestSubcommand))
            {
                MetadataProviderFactory = () =>
                {
                    providerCreated = true;
                    return new ReflectionMetadataProvider(typeof(TestSubcommand));
                }
            };

            Assert.False(providerCreated);

            var provider = metadata.MetadataProviderFactory!();

            Assert.True(providerCreated);
            Assert.NotNull(provider);
            Assert.Equal(typeof(TestSubcommand), provider.ModelType);
        }

        [Fact]
        public void MultipleSubcommands_CanBeCreated()
        {
            var testMeta = new SubcommandMetadata(typeof(TestSubcommand))
            {
                CommandName = "test"
            };

            var otherMeta = new SubcommandMetadata(typeof(OtherSubcommand))
            {
                CommandName = "other"
            };

            Assert.NotEqual(testMeta.SubcommandType, otherMeta.SubcommandType);
            Assert.NotEqual(testMeta.CommandName, otherMeta.CommandName);
        }

        [Fact]
        public void DifferentInstancesAreSeparate()
        {
            var metadata1 = new SubcommandMetadata(typeof(TestSubcommand))
            {
                CommandName = "cmd1"
            };

            var metadata2 = new SubcommandMetadata(typeof(TestSubcommand))
            {
                CommandName = "cmd2"
            };

            // Modifying one should not affect the other
            Assert.Equal("cmd1", metadata1.CommandName);
            Assert.Equal("cmd2", metadata2.CommandName);
        }
    }
}
