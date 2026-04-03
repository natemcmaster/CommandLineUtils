// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    /// <summary>
    /// Collection for tests that use CommandMetadataRegistry (shared state).
    /// Tests in this collection run sequentially to avoid interference.
    /// </summary>
    [CollectionDefinition("MetadataRegistry", DisableParallelization = true)]
    public class MetadataRegistryCollection { }

    [Collection("MetadataRegistry")]
    public class DefaultMetadataResolverTests : IDisposable
    {
        [Command(Name = "registered")]
        private class RegisteredCommand { }

        [Command(Name = "unregistered")]
        private class UnregisteredCommand { }

        [Command(Name = "generic")]
        private class GenericCommand { }

        public DefaultMetadataResolverTests()
        {
            // Clean up before each test
            CommandMetadataRegistry.Clear();
            DefaultMetadataResolver.Instance.ClearCache();
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            var instance1 = DefaultMetadataResolver.Instance;
            var instance2 = DefaultMetadataResolver.Instance;

            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void GetProvider_ReturnsRegisteredProvider_WhenAvailable()
        {
            var registered = new ReflectionMetadataProvider(typeof(RegisteredCommand));
            CommandMetadataRegistry.Register(typeof(RegisteredCommand), registered);

            var provider = DefaultMetadataResolver.Instance.GetProvider(typeof(RegisteredCommand));

            Assert.Same(registered, provider);
        }

        [Fact]
        public void GetProvider_FallsBackToReflection_WhenNotRegistered()
        {
            var provider = DefaultMetadataResolver.Instance.GetProvider(typeof(UnregisteredCommand));

            Assert.NotNull(provider);
            Assert.Equal(typeof(UnregisteredCommand), provider.ModelType);
            Assert.IsType<ReflectionMetadataProvider>(provider);
        }

        [Fact]
        public void GetProvider_CachesReflectionProvider()
        {
            var provider1 = DefaultMetadataResolver.Instance.GetProvider(typeof(UnregisteredCommand));
            var provider2 = DefaultMetadataResolver.Instance.GetProvider(typeof(UnregisteredCommand));

            Assert.Same(provider1, provider2);
        }

        [Fact]
        public void GetProvider_Generic_ReturnsRegisteredProvider_WhenAvailable()
        {
            var registered = new ReflectionMetadataProvider(typeof(GenericCommand));
            CommandMetadataRegistry.Register(typeof(GenericCommand), registered);

            var provider = DefaultMetadataResolver.Instance.GetProvider<GenericCommand>();

            Assert.NotNull(provider);
            Assert.Equal(typeof(GenericCommand), provider.ModelType);
        }

        [Fact]
        public void GetProvider_Generic_FallsBackToReflection_WhenNotRegistered()
        {
            var provider = DefaultMetadataResolver.Instance.GetProvider<GenericCommand>();

            Assert.NotNull(provider);
            Assert.Equal(typeof(GenericCommand), provider.ModelType);
        }

        [Fact]
        public void GetProvider_Generic_ReturnsTypedProvider()
        {
            var provider = DefaultMetadataResolver.Instance.GetProvider<GenericCommand>();

            Assert.IsAssignableFrom<ICommandMetadataProvider<GenericCommand>>(provider);
        }

        [Fact]
        public void HasGeneratedMetadata_ReturnsTrue_WhenRegistered()
        {
            var registered = new ReflectionMetadataProvider(typeof(RegisteredCommand));
            CommandMetadataRegistry.Register(typeof(RegisteredCommand), registered);

            Assert.True(DefaultMetadataResolver.Instance.HasGeneratedMetadata(typeof(RegisteredCommand)));
        }

        [Fact]
        public void HasGeneratedMetadata_ReturnsFalse_WhenNotRegistered()
        {
            Assert.False(DefaultMetadataResolver.Instance.HasGeneratedMetadata(typeof(UnregisteredCommand)));
        }

        [Fact]
        public void ClearCache_RemovesCachedReflectionProviders()
        {
            var provider1 = DefaultMetadataResolver.Instance.GetProvider(typeof(UnregisteredCommand));

            DefaultMetadataResolver.Instance.ClearCache();

            var provider2 = DefaultMetadataResolver.Instance.GetProvider(typeof(UnregisteredCommand));

            Assert.NotSame(provider1, provider2);
        }

        [Fact]
        public void ImplementsIMetadataResolver()
        {
            Assert.IsAssignableFrom<IMetadataResolver>(DefaultMetadataResolver.Instance);
        }

        public void Dispose()
        {
            CommandMetadataRegistry.Clear();
            DefaultMetadataResolver.Instance.ClearCache();
        }
    }
}
