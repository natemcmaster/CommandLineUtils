// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Conventions;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    /// <summary>
    /// Tests for the AOT-friendly code paths in conventions.
    /// These tests exercise the generated metadata paths in conventions.
    /// </summary>
    public class ConventionAotPathTests : IDisposable
    {
        public ConventionAotPathTests()
        {
            // Clean up registry before each test
            CommandMetadataRegistry.Clear();
        }

        public void Dispose()
        {
            // Clean up registry after each test
            CommandMetadataRegistry.Clear();
        }

        #region Test Models

        private class ParentModel
        {
            public string? Name { get; set; }
        }

        private class ChildModel
        {
            public ParentModel? Parent { get; set; }
            public object? Subcommand { get; set; }
            public string[]? RemainingArguments { get; set; }
        }

        private class SubcommandModel
        {
            public string? Value { get; set; }
        }

        #endregion

        #region Mock Metadata Provider

        /// <summary>
        /// A mock metadata provider that provides SpecialPropertiesMetadata
        /// to exercise the AOT code paths in conventions.
        /// </summary>
        private class MockMetadataProviderWithSpecialProperties : ICommandMetadataProvider
        {
            public Type ModelType { get; }
            public IReadOnlyList<OptionMetadata> Options { get; } = Array.Empty<OptionMetadata>();
            public IReadOnlyList<ArgumentMetadata> Arguments { get; } = Array.Empty<ArgumentMetadata>();
            public IReadOnlyList<SubcommandMetadata> Subcommands { get; } = Array.Empty<SubcommandMetadata>();
            public CommandMetadata? CommandInfo { get; set; }
            public IExecuteHandler? ExecuteHandler { get; set; }
            public IValidateHandler? ValidateHandler { get; set; }
            public IValidationErrorHandler? ValidationErrorHandler { get; set; }
            public SpecialPropertiesMetadata? SpecialProperties { get; set; }
            public HelpOptionMetadata? HelpOption { get; set; }
            public VersionOptionMetadata? VersionOption { get; set; }

            public MockMetadataProviderWithSpecialProperties(Type modelType)
            {
                ModelType = modelType;
            }

            public IModelFactory GetModelFactory(IServiceProvider? services)
            {
                return new ActivatorModelFactory(ModelType);
            }
        }

        #endregion

        #region RemainingArgsPropertyConvention AOT Path Tests

        [Fact]
        public void RemainingArgsPropertyConvention_UsesGeneratedSetter_WhenAvailable()
        {
            // Arrange - register a mock provider with SpecialProperties
            var provider = new MockMetadataProviderWithSpecialProperties(typeof(ChildModel))
            {
                SpecialProperties = new SpecialPropertiesMetadata
                {
                    RemainingArgumentsSetter = (obj, val) => ((ChildModel)obj).RemainingArguments = (string[]?)val,
                    RemainingArgumentsType = typeof(string[])
                }
            };
            CommandMetadataRegistry.Register(typeof(ChildModel), provider);

            var app = new CommandLineApplication<ChildModel>();
            app.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect;
            app.Conventions.AddConvention(new RemainingArgsPropertyConvention());

            // Act
            app.Parse("arg1", "arg2", "arg3");

            // Assert
            Assert.NotNull(app.Model.RemainingArguments);
            Assert.Equal(new[] { "arg1", "arg2", "arg3" }, app.Model.RemainingArguments);
        }

        [Fact]
        public void RemainingArgsPropertyConvention_UsesGeneratedSetter_WithListType()
        {
            // Create a model that uses List<string> for remaining args
            var provider = new MockMetadataProviderWithSpecialProperties(typeof(RemainingArgsListModel))
            {
                SpecialProperties = new SpecialPropertiesMetadata
                {
                    RemainingArgumentsSetter = (obj, val) => ((RemainingArgsListModel)obj).RemainingArgs = (List<string>?)val,
                    RemainingArgumentsType = typeof(List<string>)
                }
            };
            CommandMetadataRegistry.Register(typeof(RemainingArgsListModel), provider);

            var app = new CommandLineApplication<RemainingArgsListModel>();
            app.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect;
            app.Conventions.AddConvention(new RemainingArgsPropertyConvention());

            // Act
            app.Parse("a", "b");

            // Assert
            Assert.NotNull(app.Model.RemainingArgs);
            Assert.Equal(new[] { "a", "b" }, app.Model.RemainingArgs);
        }

        private class RemainingArgsListModel
        {
            public List<string>? RemainingArgs { get; set; }
        }

        #endregion

        #region ParentPropertyConvention AOT Path Tests

        [Fact]
        public void ParentPropertyConvention_UsesGeneratedSetter_WhenAvailable()
        {
            // Arrange
            var childProvider = new MockMetadataProviderWithSpecialProperties(typeof(ChildModel))
            {
                SpecialProperties = new SpecialPropertiesMetadata
                {
                    ParentSetter = (obj, val) => ((ChildModel)obj).Parent = (ParentModel?)val,
                    ParentType = typeof(ParentModel)
                }
            };
            CommandMetadataRegistry.Register(typeof(ChildModel), childProvider);

            var app = new CommandLineApplication<ParentModel>();
            var subApp = app.Command<ChildModel>("child", _ => { });
            subApp.Conventions.AddConvention(new ParentPropertyConvention());

            // Act
            app.Parse("child");

            // Assert - Parent should be set via the generated setter
            Assert.NotNull(subApp.Model.Parent);
            Assert.Same(app.Model, subApp.Model.Parent);
        }

        #endregion

        #region SubcommandPropertyConvention AOT Path Tests

        [Fact]
        public void SubcommandPropertyConvention_UsesGeneratedSetter_WhenAvailable()
        {
            // Arrange
            var parentProvider = new MockMetadataProviderWithSpecialProperties(typeof(ChildModel))
            {
                SpecialProperties = new SpecialPropertiesMetadata
                {
                    SubcommandSetter = (obj, val) => ((ChildModel)obj).Subcommand = val,
                    SubcommandType = typeof(object)
                }
            };
            CommandMetadataRegistry.Register(typeof(ChildModel), parentProvider);

            var app = new CommandLineApplication<ChildModel>();
            var subApp = app.Command<SubcommandModel>("sub", _ => { });
            app.Conventions.AddConvention(new SubcommandPropertyConvention());

            // Act
            app.Parse("sub");

            // Assert - Subcommand should be set via the generated setter
            Assert.NotNull(app.Model.Subcommand);
            Assert.IsType<SubcommandModel>(app.Model.Subcommand);
        }

        #endregion

        #region ConventionContext Tests

        [Fact]
        public void ConventionContext_MetadataProvider_ReturnsRegisteredProvider()
        {
            // Arrange
            var provider = new MockMetadataProviderWithSpecialProperties(typeof(ChildModel));
            CommandMetadataRegistry.Register(typeof(ChildModel), provider);

            var app = new CommandLineApplication<ChildModel>();
            var context = new ConventionContext(app, typeof(ChildModel));

            // Act
            var metadataProvider = context.MetadataProvider;

            // Assert
            Assert.NotNull(metadataProvider);
        }

        [Fact]
        public void ConventionContext_HasGeneratedMetadata_ReturnsTrue_WhenRegistered()
        {
            // Arrange
            var provider = new MockMetadataProviderWithSpecialProperties(typeof(ChildModel));
            CommandMetadataRegistry.Register(typeof(ChildModel), provider);

            var app = new CommandLineApplication<ChildModel>();
            var context = new ConventionContext(app, typeof(ChildModel));

            // Act
            var hasGenerated = context.HasGeneratedMetadata;

            // Assert
            Assert.True(hasGenerated);
        }

        [Fact]
        public void ConventionContext_HasGeneratedMetadata_ReturnsFalse_WhenNotRegistered()
        {
            // Arrange - Don't register any provider
            var app = new CommandLineApplication<ChildModel>();
            var context = new ConventionContext(app, typeof(ChildModel));

            // Act
            var hasGenerated = context.HasGeneratedMetadata;

            // Assert - Falls back to reflection, which is not "generated"
            Assert.False(hasGenerated);
        }

        [Fact]
        public void ConventionContext_HasGeneratedMetadata_ReturnsFalse_WhenModelTypeIsNull()
        {
            // Arrange
            var app = new CommandLineApplication();
            var context = new ConventionContext(app, null);

            // Act
            var hasGenerated = context.HasGeneratedMetadata;

            // Assert
            Assert.False(hasGenerated);
        }

        [Fact]
        public void ConventionContext_MetadataProvider_CachesResult()
        {
            // Arrange
            var provider = new MockMetadataProviderWithSpecialProperties(typeof(ChildModel));
            CommandMetadataRegistry.Register(typeof(ChildModel), provider);

            var app = new CommandLineApplication<ChildModel>();
            var context = new ConventionContext(app, typeof(ChildModel));

            // Act
            var first = context.MetadataProvider;
            var second = context.MetadataProvider;

            // Assert
            Assert.Same(first, second);
        }

        #endregion

        #region CommandAttributeConvention AOT Path Tests

        [Fact]
        public void CommandAttributeConvention_UsesMetadataProvider_WhenAvailable()
        {
            // Arrange
            var commandInfo = new CommandMetadata
            {
                Name = "test-cmd",
                Description = "A test command"
            };
            var provider = new MockMetadataProviderWithSpecialProperties(typeof(ChildModel))
            {
                CommandInfo = commandInfo
            };
            CommandMetadataRegistry.Register(typeof(ChildModel), provider);

            var app = new CommandLineApplication<ChildModel>();
            app.Conventions.AddConvention(new CommandAttributeConvention());

            // Assert - Command info should be applied
            Assert.Equal("test-cmd", app.Name);
            Assert.Equal("A test command", app.Description);
        }

        [Fact]
        public void CommandAttributeConvention_AppliesCommandMetadata_WithAllProperties()
        {
            // Arrange - ClusterOptions is nullable, so setting it means it "was set"
            var commandInfo = new CommandMetadata
            {
                Name = "full-cmd",
                Description = "Full description",
                FullName = "Full Name",
                ExtendedHelpText = "Extended help",
                ShowInHelpText = false,
                AllowArgumentSeparator = true,
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
                ClusterOptions = false // Setting it means it was explicitly set
            };
            var provider = new MockMetadataProviderWithSpecialProperties(typeof(ChildModel))
            {
                CommandInfo = commandInfo
            };
            CommandMetadataRegistry.Register(typeof(ChildModel), provider);

            var app = new CommandLineApplication<ChildModel>();
            app.Conventions.AddConvention(new CommandAttributeConvention());

            // Assert
            Assert.Equal("full-cmd", app.Name);
            Assert.Equal("Full description", app.Description);
            Assert.Equal("Full Name", app.FullName);
            Assert.Equal("Extended help", app.ExtendedHelpText);
            Assert.False(app.ShowInHelpText);
            Assert.True(app.AllowArgumentSeparator);
            Assert.Equal(ResponseFileHandling.ParseArgsAsLineSeparated, app.ResponseFileHandling);
            Assert.False(app.ClusterOptions);
        }

        #endregion

        #region SubcommandAttributeConvention AOT Path Tests

        [Fact]
        public void SubcommandAttributeConvention_UsesMetadataProvider_WhenAvailable()
        {
            // Arrange - SubcommandMetadata requires subcommandType in constructor
            var subMeta = new SubcommandMetadata(typeof(SubcommandModel))
            {
                MetadataProviderFactory = () => new MockMetadataProviderWithSpecialProperties(typeof(SubcommandModel))
                {
                    CommandInfo = new CommandMetadata { Name = "sub-from-meta" }
                }
            };

            // Create a custom provider class that has mutable Subcommands
            var mockProvider = new MockProviderWithSubcommands(typeof(ChildModel));
            mockProvider.SetSubcommands(new[] { subMeta });
            CommandMetadataRegistry.Register(typeof(ChildModel), mockProvider);

            var app = new CommandLineApplication<ChildModel>();
            app.Conventions.AddConvention(new SubcommandAttributeConvention());

            // Assert - Use First() since Commands is IReadOnlyCollection
            Assert.Single(app.Commands);
            Assert.Equal("sub-from-meta", app.Commands.First().Name);
        }

        private class MockProviderWithSubcommands : ICommandMetadataProvider
        {
            private IReadOnlyList<SubcommandMetadata> _subcommands = Array.Empty<SubcommandMetadata>();

            public Type ModelType { get; }
            public IReadOnlyList<OptionMetadata> Options => Array.Empty<OptionMetadata>();
            public IReadOnlyList<ArgumentMetadata> Arguments => Array.Empty<ArgumentMetadata>();
            public IReadOnlyList<SubcommandMetadata> Subcommands => _subcommands;
            public CommandMetadata? CommandInfo { get; set; }
            public IExecuteHandler? ExecuteHandler { get; set; }
            public IValidateHandler? ValidateHandler { get; set; }
            public IValidationErrorHandler? ValidationErrorHandler { get; set; }
            public SpecialPropertiesMetadata? SpecialProperties { get; set; }
            public HelpOptionMetadata? HelpOption { get; set; }
            public VersionOptionMetadata? VersionOption { get; set; }

            public MockProviderWithSubcommands(Type modelType)
            {
                ModelType = modelType;
            }

            public void SetSubcommands(IReadOnlyList<SubcommandMetadata> subcommands)
            {
                _subcommands = subcommands;
            }

            public IModelFactory GetModelFactory(IServiceProvider? services)
            {
                return new ActivatorModelFactory(ModelType);
            }
        }

        #endregion
    }
}
