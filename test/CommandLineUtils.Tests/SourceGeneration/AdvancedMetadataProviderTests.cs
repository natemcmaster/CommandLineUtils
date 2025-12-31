// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    /// <summary>
    /// Tests for advanced metadata provider features including:
    /// - SpecialPropertiesMetadata (Parent, Subcommand, RemainingArguments)
    /// - Type-level HelpOption and VersionOption
    /// - VersionOptionFromMember
    /// - Constructor injection
    /// - Command name inference
    /// </summary>
    public class AdvancedMetadataProviderTests
    {
        #region SpecialPropertiesMetadata Tests

        [Command(Name = "parent")]
        [Subcommand(typeof(ChildWithParent))]
        private class ParentWithSubcommandProperty
        {
            public object? Subcommand { get; set; }
        }

        [Command(Name = "child")]
        private class ChildWithParent
        {
            public ParentWithSubcommandProperty? Parent { get; set; }
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsParentProperty()
        {
            var provider = new ReflectionMetadataProvider(typeof(ChildWithParent));

            Assert.NotNull(provider.SpecialProperties);
            Assert.NotNull(provider.SpecialProperties!.ParentSetter);
            Assert.Equal(typeof(ParentWithSubcommandProperty), provider.SpecialProperties.ParentType);
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsSubcommandProperty()
        {
            var provider = new ReflectionMetadataProvider(typeof(ParentWithSubcommandProperty));

            Assert.NotNull(provider.SpecialProperties);
            Assert.NotNull(provider.SpecialProperties!.SubcommandSetter);
            Assert.Equal(typeof(object), provider.SpecialProperties.SubcommandType);
        }

        [Command(Name = "echo", UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue)]
        private class CommandWithRemainingArgs
        {
            public string[]? RemainingArguments { get; set; }
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsRemainingArgumentsProperty()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithRemainingArgs));

            Assert.NotNull(provider.SpecialProperties);
            Assert.NotNull(provider.SpecialProperties!.RemainingArgumentsSetter);
            Assert.Equal(typeof(string[]), provider.SpecialProperties.RemainingArgumentsType);
        }

        [Fact]
        public void SpecialProperties_ParentSetter_SetsValue()
        {
            var provider = new ReflectionMetadataProvider(typeof(ChildWithParent));
            var child = new ChildWithParent();
            var parent = new ParentWithSubcommandProperty();

            provider.SpecialProperties!.ParentSetter!(child, parent);

            Assert.Same(parent, child.Parent);
        }

        [Fact]
        public void SpecialProperties_SubcommandSetter_SetsValue()
        {
            var provider = new ReflectionMetadataProvider(typeof(ParentWithSubcommandProperty));
            var parent = new ParentWithSubcommandProperty();
            var child = new ChildWithParent();

            provider.SpecialProperties!.SubcommandSetter!(parent, child);

            Assert.Same(child, parent.Subcommand);
        }

        [Fact]
        public void SpecialProperties_RemainingArgumentsSetter_SetsValue()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithRemainingArgs));
            var cmd = new CommandWithRemainingArgs();
            var args = new[] { "arg1", "arg2" };

            provider.SpecialProperties!.RemainingArgumentsSetter!(cmd, args);

            Assert.Equal(args, cmd.RemainingArguments);
        }

        #endregion

        #region Type-Level HelpOption and VersionOption Tests

        [Command(Name = "app")]
        [HelpOption("-h|--help")]
        private class CommandWithTypeLevelHelpOption
        {
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsTypeLevelHelpOption()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithTypeLevelHelpOption));

            Assert.NotNull(provider.HelpOption);
            Assert.Equal("-h|--help", provider.HelpOption!.Template);
        }

        [Command(Name = "app")]
        [VersionOption("1.0.0")]
        private class CommandWithTypeLevelVersionOption
        {
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsTypeLevelVersionOption()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithTypeLevelVersionOption));

            Assert.NotNull(provider.VersionOption);
            Assert.Equal("1.0.0", provider.VersionOption!.Version);
        }

        [Command(Name = "app")]
        [VersionOption("-v|--version", "2.0.0")]
        private class CommandWithVersionOptionTemplateAndVersion
        {
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsVersionOptionWithTemplate()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithVersionOptionTemplateAndVersion));

            Assert.NotNull(provider.VersionOption);
            Assert.Equal("-v|--version", provider.VersionOption!.Template);
            Assert.Equal("2.0.0", provider.VersionOption.Version);
        }

        #endregion

        #region VersionOptionFromMember Tests

        [Command(Name = "app")]
        [VersionOptionFromMember(MemberName = nameof(GetVersion))]
        private class CommandWithVersionFromMember
        {
            public string GetVersion => "3.0.0-dynamic";
        }

        [Fact]
        public void VersionOptionFromMember_WorksWithConventions()
        {
            var app = new CommandLineApplication<CommandWithVersionFromMember>();
            app.Conventions.UseDefaultConventions();

            // Verify version option was configured
            Assert.NotNull(app.OptionVersion);
        }

        [Command(Name = "app")]
        [VersionOptionFromMember("-V|--ver", MemberName = nameof(Version))]
        private class CommandWithVersionFromMemberAndTemplate
        {
            public string Version => "4.0.0";
        }

        [Fact]
        public void VersionOptionFromMember_WorksWithCustomTemplate()
        {
            var app = new CommandLineApplication<CommandWithVersionFromMemberAndTemplate>();
            app.Conventions.UseDefaultConventions();

            // Verify version option was configured
            Assert.NotNull(app.OptionVersion);
            // ShortName is just "V" (without the dash)
            Assert.Equal("V", app.OptionVersion!.ShortName);
        }

        #endregion

        #region Constructor Injection Tests

        public interface ITestService
        {
            string GetMessage();
        }

        private class TestService : ITestService
        {
            public string GetMessage() => "Hello from service";
        }

        [Command(Name = "inject")]
        private class CommandWithConstructorInjection
        {
            private readonly ITestService _service;

            public CommandWithConstructorInjection(ITestService service)
            {
                _service = service;
            }

            public string GetServiceMessage() => _service.GetMessage();
        }

        private class SimpleServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _services = new();

            public void Register<T>(T instance) where T : notnull
            {
                _services[typeof(T)] = instance;
            }

            public object? GetService(Type serviceType)
            {
                return _services.TryGetValue(serviceType, out var service) ? service : null;
            }
        }

        [Fact]
        public void ConstructorInjection_WorksWithServices()
        {
            var services = new SimpleServiceProvider();
            var testService = new TestService();
            services.Register<ITestService>(testService);

            var app = new CommandLineApplication<CommandWithConstructorInjection>();
            app.Conventions.UseConstructorInjection(services);

            Assert.NotNull(app.Model);
            Assert.Equal("Hello from service", app.Model.GetServiceMessage());
        }

        [Command(Name = "multi")]
        private class CommandWithMultipleConstructors
        {
            public ITestService? Service { get; }
            public bool UsedParameterlessConstructor { get; }

            public CommandWithMultipleConstructors()
            {
                UsedParameterlessConstructor = true;
            }

            public CommandWithMultipleConstructors(ITestService service)
            {
                Service = service;
                UsedParameterlessConstructor = false;
            }
        }

        [Fact]
        public void ConstructorInjection_PrefersParameterizedConstructor()
        {
            var services = new SimpleServiceProvider();
            var testService = new TestService();
            services.Register<ITestService>(testService);

            var app = new CommandLineApplication<CommandWithMultipleConstructors>();
            app.Conventions.UseConstructorInjection(services);

            Assert.NotNull(app.Model);
            Assert.False(app.Model.UsedParameterlessConstructor);
            Assert.Same(testService, app.Model.Service);
        }

        [Fact]
        public void ConstructorInjection_FallsBackToParameterlessConstructor()
        {
            var services = new SimpleServiceProvider();
            // Not registering ITestService

            var app = new CommandLineApplication<CommandWithMultipleConstructors>();
            app.Conventions.UseConstructorInjection(services);

            Assert.NotNull(app.Model);
            Assert.True(app.Model.UsedParameterlessConstructor);
            Assert.Null(app.Model.Service);
        }

        #endregion

        #region Command Name Inference Tests

        [Command(Name = "parent")]
        [Subcommand(typeof(MyTestCommand), typeof(AddInferredCommand), typeof(RemoveItemCommand), typeof(ExplicitNameCommand))]
        private class NameInferenceParent
        {
        }

        [Command(Description = "No explicit name")]
        private class MyTestCommand
        {
        }

        [Fact]
        public void CommandNameFromType_InfersName()
        {
            var app = new CommandLineApplication<NameInferenceParent>();
            app.Conventions.UseDefaultConventions();

            var sub = app.Commands.FirstOrDefault(c => c.Name == "my-test");
            Assert.NotNull(sub);
        }

        [Command(Description = "Should become 'add-inferred'")]
        private class AddInferredCommand
        {
        }

        [Fact]
        public void CommandNameFromType_StripsCommandSuffix()
        {
            var app = new CommandLineApplication<NameInferenceParent>();
            app.Conventions.UseDefaultConventions();

            var sub = app.Commands.FirstOrDefault(c => c.Name == "add-inferred");
            Assert.NotNull(sub);
        }

        [Command(Description = "Should become 'remove-item'")]
        private class RemoveItemCommand
        {
        }

        [Fact]
        public void CommandNameFromType_ConvertsToKebabCase()
        {
            var app = new CommandLineApplication<NameInferenceParent>();
            app.Conventions.UseDefaultConventions();

            var sub = app.Commands.FirstOrDefault(c => c.Name == "remove-item");
            Assert.NotNull(sub);
        }

        [Command(Name = "explicit-name", Description = "Explicit name should be used")]
        private class ExplicitNameCommand
        {
        }

        [Fact]
        public void CommandNameFromType_PreservesExplicitName()
        {
            var app = new CommandLineApplication<NameInferenceParent>();
            app.Conventions.UseDefaultConventions();

            var sub = app.Commands.FirstOrDefault(c => c.Name == "explicit-name");
            Assert.NotNull(sub);
        }

        #endregion

        #region Integration Tests

        [Command(Name = "parent")]
        [HelpOption("-?|-h|--help")]
        [VersionOptionFromMember(MemberName = nameof(Version))]
        [Subcommand(typeof(IntegrationChildCommand))]
        private class IntegrationParentCommand
        {
            public string Version => "1.0.0-integration";
            public object? Subcommand { get; set; }
        }

        [Command(Name = "child")]
        private class IntegrationChildCommand
        {
            public IntegrationParentCommand? Parent { get; set; }

            [Option("-n|--name")]
            public string? Name { get; set; }
        }

        [Fact]
        public void Integration_AllFeaturesWorkTogether()
        {
            var app = new CommandLineApplication<IntegrationParentCommand>();
            app.Conventions.UseDefaultConventions();

            // Parse a command
            var result = app.Parse("child", "-n", "test");

            // Verify parent command
            Assert.NotNull(app.Model);

            // Verify subcommand was set
            Assert.NotNull(app.Model.Subcommand);
            var child = Assert.IsType<IntegrationChildCommand>(app.Model.Subcommand);

            // Verify parent was set on child
            Assert.Same(app.Model, child.Parent);

            // Verify option was parsed
            Assert.Equal("test", child.Name);
        }

        [Fact]
        public void Integration_HelpOptionWorks()
        {
            var app = new CommandLineApplication<IntegrationParentCommand>();
            app.Conventions.UseDefaultConventions();

            // Verify help option was configured
            Assert.NotNull(app.OptionHelp);
        }

        [Fact]
        public void Integration_VersionOptionWorks()
        {
            var app = new CommandLineApplication<IntegrationParentCommand>();
            app.Conventions.UseDefaultConventions();

            // Verify version option was configured
            Assert.NotNull(app.OptionVersion);
        }

        #endregion
    }
}
