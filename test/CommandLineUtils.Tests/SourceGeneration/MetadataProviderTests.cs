// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class MetadataProviderTests
    {
        [Command(Name = "test", Description = "A test command")]
        private class TestCommand
        {
            [Option("-n|--name", Description = "The name")]
            public string? Name { get; set; }

            [Option("-v|--verbose", Description = "Verbose output")]
            public bool Verbose { get; set; }

            [Argument(0, Name = "file", Description = "The file to process")]
            public string? File { get; set; }
        }

        [Command(Name = "parent")]
        [Subcommand(typeof(ChildCommand))]
        private class ParentCommand
        {
            [Option("-g|--global")]
            public bool Global { get; set; }
        }

        [Command(Name = "child")]
        private class ChildCommand
        {
            [Option("-l|--local")]
            public bool Local { get; set; }
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsCommandInfo()
        {
            var provider = new ReflectionMetadataProvider(typeof(TestCommand));

            Assert.Equal(typeof(TestCommand), provider.ModelType);
            Assert.NotNull(provider.CommandInfo);
            Assert.Equal("test", provider.CommandInfo!.Name);
            Assert.Equal("A test command", provider.CommandInfo.Description);
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsOptions()
        {
            var provider = new ReflectionMetadataProvider(typeof(TestCommand));

            Assert.Equal(2, provider.Options.Count);

            var nameOption = provider.Options.FirstOrDefault(o => o.PropertyName == "Name");
            Assert.NotNull(nameOption);
            Assert.Equal("-n|--name", nameOption!.Template);
            Assert.Equal("The name", nameOption.Description);
            Assert.Equal(typeof(string), nameOption.PropertyType);

            var verboseOption = provider.Options.FirstOrDefault(o => o.PropertyName == "Verbose");
            Assert.NotNull(verboseOption);
            Assert.Equal("-v|--verbose", verboseOption!.Template);
            Assert.Equal(typeof(bool), verboseOption.PropertyType);
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsArguments()
        {
            var provider = new ReflectionMetadataProvider(typeof(TestCommand));

            Assert.Single(provider.Arguments);

            var fileArg = provider.Arguments[0];
            Assert.Equal("File", fileArg.PropertyName);
            Assert.Equal("file", fileArg.Name);
            Assert.Equal("The file to process", fileArg.Description);
            Assert.Equal(0, fileArg.Order);
            Assert.Equal(typeof(string), fileArg.PropertyType);
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsSubcommands()
        {
            var provider = new ReflectionMetadataProvider(typeof(ParentCommand));

            Assert.Single(provider.Subcommands);
            Assert.Equal(typeof(ChildCommand), provider.Subcommands[0].SubcommandType);
        }

        [Fact]
        public void ReflectionMetadataProvider_GetterAndSetterWork()
        {
            var provider = new ReflectionMetadataProvider(typeof(TestCommand));
            var instance = new TestCommand();

            var nameOption = provider.Options.First(o => o.PropertyName == "Name");

            // Test setter
            nameOption.Setter(instance, "test-value");
            Assert.Equal("test-value", instance.Name);

            // Test getter
            var value = nameOption.Getter(instance);
            Assert.Equal("test-value", value);
        }

        [Fact]
        public void CommandMetadataRegistry_RegisterAndRetrieve()
        {
            // Clear any existing registrations
            CommandMetadataRegistry.Clear();

            var provider = new ReflectionMetadataProvider(typeof(TestCommand));
            CommandMetadataRegistry.Register(typeof(TestCommand), provider);

            Assert.True(CommandMetadataRegistry.HasMetadata(typeof(TestCommand)));
            Assert.True(CommandMetadataRegistry.TryGetProvider(typeof(TestCommand), out var retrieved));
            Assert.Same(provider, retrieved);

            // Clean up
            CommandMetadataRegistry.Clear();
        }

        [Fact]
        public void DefaultMetadataResolver_ReturnsRegisteredProvider()
        {
            // Clear any existing registrations
            CommandMetadataRegistry.Clear();
            DefaultMetadataResolver.Instance.ClearCache();

            var provider = new ReflectionMetadataProvider(typeof(TestCommand));
            CommandMetadataRegistry.Register(typeof(TestCommand), provider);

            var resolved = DefaultMetadataResolver.Instance.GetProvider(typeof(TestCommand));
            Assert.Same(provider, resolved);

            // Clean up
            CommandMetadataRegistry.Clear();
            DefaultMetadataResolver.Instance.ClearCache();
        }

        [Fact]
        public void DefaultMetadataResolver_FallsBackToReflection()
        {
            // Clear any existing registrations
            CommandMetadataRegistry.Clear();
            DefaultMetadataResolver.Instance.ClearCache();

            // No registration, should create reflection provider
            var resolved = DefaultMetadataResolver.Instance.GetProvider(typeof(TestCommand));

            Assert.NotNull(resolved);
            Assert.Equal(typeof(TestCommand), resolved.ModelType);
            Assert.NotNull(resolved.CommandInfo);
            Assert.Equal("test", resolved.CommandInfo!.Name);

            // Clean up
            CommandMetadataRegistry.Clear();
            DefaultMetadataResolver.Instance.ClearCache();
        }

        [Fact]
        public void DefaultMetadataResolver_HasGeneratedMetadata_ReturnsFalse_WhenNotRegistered()
        {
            CommandMetadataRegistry.Clear();

            Assert.False(DefaultMetadataResolver.Instance.HasGeneratedMetadata(typeof(TestCommand)));

            CommandMetadataRegistry.Clear();
        }

        [Fact]
        public void DefaultMetadataResolver_HasGeneratedMetadata_ReturnsTrue_WhenRegistered()
        {
            CommandMetadataRegistry.Clear();

            var provider = new ReflectionMetadataProvider(typeof(TestCommand));
            CommandMetadataRegistry.Register(typeof(TestCommand), provider);

            Assert.True(DefaultMetadataResolver.Instance.HasGeneratedMetadata(typeof(TestCommand)));

            CommandMetadataRegistry.Clear();
        }

        [Fact]
        public void CommandMetadata_ApplyTo_SetsProperties()
        {
            var metadata = new CommandMetadata
            {
                Name = "myapp",
                Description = "My application",
                FullName = "My Full Application Name",
                ExtendedHelpText = "Extended help here"
            };

            var app = new CommandLineApplication();
            metadata.ApplyTo(app);

            Assert.Equal("myapp", app.Name);
            Assert.Equal("My application", app.Description);
            Assert.Equal("My Full Application Name", app.FullName);
            Assert.Equal("Extended help here", app.ExtendedHelpText);
        }

        [Fact]
        public void ModelFactory_CreatesInstance()
        {
            var provider = new ReflectionMetadataProvider(typeof(TestCommand));
            var factory = provider.GetModelFactory(null);

            var instance = factory.Create();

            Assert.NotNull(instance);
            Assert.IsType<TestCommand>(instance);
        }
    }
}
