// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class ReflectionMetadataProviderIntegrationTests
    {
        [Command(
            Name = "full-command",
            Description = "A fully featured command",
            FullName = "Full Command Name",
            ExtendedHelpText = "Extended help text here")]
        [Subcommand(typeof(SubCmd1), typeof(SubCmd2))]
        [HelpOption("-h|--help")]
        private class FullFeaturedCommand
        {
            [Option("-n|--name", Description = "Your name")]
            [Required]
            public string? Name { get; set; }

            [Option("-v|--verbose", Description = "Enable verbose output")]
            public bool Verbose { get; set; }

            [Option("-c|--count", Description = "A count value")]
            [Range(1, 100)]
            public int Count { get; set; } = 1;

            [Argument(0, Name = "file", Description = "Input file")]
            [Required]
            public string? File { get; set; }

            [Argument(1, Name = "output", Description = "Output file")]
            public string? Output { get; set; }

            public object? Subcommand { get; set; }

            public int OnExecute()
            {
                return 0;
            }
        }

        [Command(Name = "sub1")]
        private class SubCmd1
        {
            public FullFeaturedCommand? Parent { get; set; }
        }

        [Command(Name = "sub2")]
        private class SubCmd2 { }

        [Command(Name = "async-command")]
        private class AsyncCommand
        {
            public async Task<int> OnExecuteAsync()
            {
                await Task.Delay(1);
                return 42;
            }
        }

        [Command(Name = "validating-command")]
        private class ValidatingCommand
        {
            public ValidationResult? OnValidate()
            {
                return ValidationResult.Success;
            }

            public int OnValidationError(ValidationResult result)
            {
                return 99;
            }
        }

        [VersionOption("--version", "1.0.0", Description = "Show version")]
        private class CommandWithVersion
        {
            public int OnExecute() => 0;
        }

        [Fact]
        public void ExtractsAllOptions()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));

            Assert.Equal(3, provider.Options.Count);

            var nameOption = provider.Options.First(o => o.PropertyName == "Name");
            Assert.Equal("-n|--name", nameOption.Template);
            Assert.Equal("Your name", nameOption.Description);
            Assert.Single(nameOption.Validators);
            Assert.IsType<RequiredAttribute>(nameOption.Validators[0]);

            var verboseOption = provider.Options.First(o => o.PropertyName == "Verbose");
            Assert.Equal("-v|--verbose", verboseOption.Template);
            Assert.Equal(typeof(bool), verboseOption.PropertyType);

            var countOption = provider.Options.First(o => o.PropertyName == "Count");
            Assert.Equal("-c|--count", countOption.Template);
            Assert.Single(countOption.Validators); // Just the Range attribute
            Assert.IsType<RangeAttribute>(countOption.Validators[0]);
        }

        [Fact]
        public void ExtractsAllArguments()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));

            Assert.Equal(2, provider.Arguments.Count);

            var fileArg = provider.Arguments.First(a => a.PropertyName == "File");
            Assert.Equal("file", fileArg.Name);
            Assert.Equal(0, fileArg.Order);
            Assert.Single(fileArg.Validators);

            var outputArg = provider.Arguments.First(a => a.PropertyName == "Output");
            Assert.Equal("output", outputArg.Name);
            Assert.Equal(1, outputArg.Order);
        }

        [Fact]
        public void ExtractsSubcommands()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));

            Assert.Equal(2, provider.Subcommands.Count);
            Assert.Contains(provider.Subcommands, s => s.SubcommandType == typeof(SubCmd1));
            Assert.Contains(provider.Subcommands, s => s.SubcommandType == typeof(SubCmd2));
        }

        [Fact]
        public void ExtractsCommandInfo()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));

            Assert.NotNull(provider.CommandInfo);
            Assert.Equal("full-command", provider.CommandInfo!.Name);
            Assert.Equal("A fully featured command", provider.CommandInfo.Description);
            Assert.Equal("Full Command Name", provider.CommandInfo.FullName);
            Assert.Equal("Extended help text here", provider.CommandInfo.ExtendedHelpText);
        }

        [Fact]
        public void ExtractsHelpOption()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));

            Assert.NotNull(provider.HelpOption);
            Assert.Equal("-h|--help", provider.HelpOption!.Template);
        }

        [Fact]
        public void ExtractsVersionOption()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithVersion));

            Assert.NotNull(provider.VersionOption);
            Assert.Equal("--version", provider.VersionOption!.Template);
            Assert.Equal("1.0.0", provider.VersionOption.Version);
            Assert.Equal("Show version", provider.VersionOption.Description);
        }

        [Fact]
        public void ExtractsSyncExecuteHandler()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));

            Assert.NotNull(provider.ExecuteHandler);
            Assert.False(provider.ExecuteHandler!.IsAsync);
        }

        [Fact]
        public void ExtractsAsyncExecuteHandler()
        {
            var provider = new ReflectionMetadataProvider(typeof(AsyncCommand));

            Assert.NotNull(provider.ExecuteHandler);
            Assert.True(provider.ExecuteHandler!.IsAsync);
        }

        [Fact]
        public void ExtractsValidateHandler()
        {
            var provider = new ReflectionMetadataProvider(typeof(ValidatingCommand));

            Assert.NotNull(provider.ValidateHandler);
        }

        [Fact]
        public void ExtractsValidationErrorHandler()
        {
            var provider = new ReflectionMetadataProvider(typeof(ValidatingCommand));

            Assert.NotNull(provider.ValidationErrorHandler);
        }

        [Fact]
        public void ExtractsSpecialProperties()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));

            Assert.NotNull(provider.SpecialProperties);
            Assert.NotNull(provider.SpecialProperties!.SubcommandSetter);
            Assert.Equal(typeof(object), provider.SpecialProperties.SubcommandType);
        }

        [Fact]
        public void ExtractsParentProperty()
        {
            var provider = new ReflectionMetadataProvider(typeof(SubCmd1));

            Assert.NotNull(provider.SpecialProperties);
            Assert.NotNull(provider.SpecialProperties!.ParentSetter);
            Assert.Equal(typeof(FullFeaturedCommand), provider.SpecialProperties.ParentType);
        }

        [Fact]
        public void GetModelFactory_CreatesInstances()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));
            var factory = provider.GetModelFactory(null);

            var instance = factory.Create();

            Assert.IsType<FullFeaturedCommand>(instance);
        }

        [Fact]
        public void PropertyAccessors_WorkCorrectly()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));
            var model = new FullFeaturedCommand();

            var nameOption = provider.Options.First(o => o.PropertyName == "Name");

            nameOption.Setter(model, "Test Name");
            Assert.Equal("Test Name", model.Name);

            var value = nameOption.Getter(model);
            Assert.Equal("Test Name", value);
        }

        [Fact]
        public void SubcommandMetadataProviderFactory_CreatesProviders()
        {
            var provider = new ReflectionMetadataProvider(typeof(FullFeaturedCommand));
            var subCmd = provider.Subcommands.First(s => s.SubcommandType == typeof(SubCmd1));

            Assert.NotNull(subCmd.MetadataProviderFactory);

            var subProvider = subCmd.MetadataProviderFactory!();

            Assert.Equal(typeof(SubCmd1), subProvider.ModelType);
            Assert.NotNull(subProvider.SpecialProperties?.ParentSetter);
        }
    }
}
