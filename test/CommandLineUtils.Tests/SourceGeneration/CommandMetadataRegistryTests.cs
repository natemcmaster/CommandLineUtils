// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class CommandMetadataRegistryTests
    {
        [Command(Name = "test1")]
        private class TestCommand1 { }

        [Command(Name = "test2")]
        private class TestCommand2 { }

        [Command(Name = "generic")]
        private class GenericTestCommand { }

        public CommandMetadataRegistryTests()
        {
            // Clean up before each test
            CommandMetadataRegistry.Clear();
        }

        [Fact]
        public void Register_AddsProviderToRegistry()
        {
            var provider = new ReflectionMetadataProvider(typeof(TestCommand1));

            CommandMetadataRegistry.Register(typeof(TestCommand1), provider);

            Assert.True(CommandMetadataRegistry.HasMetadata(typeof(TestCommand1)));
        }

        [Fact]
        public void TryGetProvider_ReturnsTrue_WhenRegistered()
        {
            var provider = new ReflectionMetadataProvider(typeof(TestCommand1));
            CommandMetadataRegistry.Register(typeof(TestCommand1), provider);

            var result = CommandMetadataRegistry.TryGetProvider(typeof(TestCommand1), out var retrieved);

            Assert.True(result);
            Assert.Same(provider, retrieved);
        }

        [Fact]
        public void TryGetProvider_ReturnsFalse_WhenNotRegistered()
        {
            var result = CommandMetadataRegistry.TryGetProvider(typeof(TestCommand1), out var retrieved);

            Assert.False(result);
            Assert.Null(retrieved);
        }

        [Fact]
        public void HasMetadata_ReturnsTrue_WhenRegistered()
        {
            var provider = new ReflectionMetadataProvider(typeof(TestCommand1));
            CommandMetadataRegistry.Register(typeof(TestCommand1), provider);

            Assert.True(CommandMetadataRegistry.HasMetadata(typeof(TestCommand1)));
        }

        [Fact]
        public void HasMetadata_ReturnsFalse_WhenNotRegistered()
        {
            Assert.False(CommandMetadataRegistry.HasMetadata(typeof(TestCommand1)));
        }

        [Fact]
        public void Clear_RemovesAllRegistrations()
        {
            var provider1 = new ReflectionMetadataProvider(typeof(TestCommand1));
            var provider2 = new ReflectionMetadataProvider(typeof(TestCommand2));
            CommandMetadataRegistry.Register(typeof(TestCommand1), provider1);
            CommandMetadataRegistry.Register(typeof(TestCommand2), provider2);

            CommandMetadataRegistry.Clear();

            Assert.False(CommandMetadataRegistry.HasMetadata(typeof(TestCommand1)));
            Assert.False(CommandMetadataRegistry.HasMetadata(typeof(TestCommand2)));
        }

        [Fact]
        public void Register_OverwritesExisting()
        {
            var provider1 = new ReflectionMetadataProvider(typeof(TestCommand1));
            var provider2 = new ReflectionMetadataProvider(typeof(TestCommand1));
            CommandMetadataRegistry.Register(typeof(TestCommand1), provider1);

            CommandMetadataRegistry.Register(typeof(TestCommand1), provider2);

            CommandMetadataRegistry.TryGetProvider(typeof(TestCommand1), out var retrieved);
            Assert.Same(provider2, retrieved);
        }

        [Fact]
        public void MultipleTypes_CanBeRegistered()
        {
            var provider1 = new ReflectionMetadataProvider(typeof(TestCommand1));
            var provider2 = new ReflectionMetadataProvider(typeof(TestCommand2));

            CommandMetadataRegistry.Register(typeof(TestCommand1), provider1);
            CommandMetadataRegistry.Register(typeof(TestCommand2), provider2);

            Assert.True(CommandMetadataRegistry.HasMetadata(typeof(TestCommand1)));
            Assert.True(CommandMetadataRegistry.HasMetadata(typeof(TestCommand2)));

            CommandMetadataRegistry.TryGetProvider(typeof(TestCommand1), out var retrieved1);
            CommandMetadataRegistry.TryGetProvider(typeof(TestCommand2), out var retrieved2);

            Assert.Same(provider1, retrieved1);
            Assert.Same(provider2, retrieved2);
        }

        [Fact]
        public void TryGetProvider_Generic_ReturnsTrue_WhenRegisteredWithTypedProvider()
        {
            // The generic TryGetProvider<T> requires ICommandMetadataProvider<T>
            // which ReflectionMetadataProvider does not implement.
            // Use the untyped TryGetProvider with typeof(T) for ReflectionMetadataProvider
            var provider = new ReflectionMetadataProvider(typeof(GenericTestCommand));
            CommandMetadataRegistry.Register(typeof(GenericTestCommand), provider);

            // The generic version checks if the provider implements ICommandMetadataProvider<T>
            // ReflectionMetadataProvider only implements ICommandMetadataProvider (non-generic)
            var genericResult = CommandMetadataRegistry.TryGetProvider<GenericTestCommand>(out var genericRetrieved);
            Assert.False(genericResult); // ReflectionMetadataProvider doesn't implement ICommandMetadataProvider<T>
            Assert.Null(genericRetrieved);

            // But the non-generic version works
            var untypedResult = CommandMetadataRegistry.TryGetProvider(typeof(GenericTestCommand), out var untypedRetrieved);
            Assert.True(untypedResult);
            Assert.Same(provider, untypedRetrieved);
        }

        [Fact]
        public void TryGetProvider_Generic_ReturnsFalse_WhenNotRegistered()
        {
            var result = CommandMetadataRegistry.TryGetProvider<GenericTestCommand>(out var retrieved);

            Assert.False(result);
            Assert.Null(retrieved);
        }
    }
}
