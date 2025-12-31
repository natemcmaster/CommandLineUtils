// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    /// <summary>
    /// Comprehensive tests for metadata providers covering edge cases and full feature parity.
    /// This ensures both ReflectionMetadataProvider and generated providers behave consistently.
    /// </summary>
    public class ComprehensiveMetadataProviderTests
    {
        #region ReflectionMetadataProvider Edge Cases

        [Fact]
        public void ReflectionMetadataProvider_CommandWithoutSpecialProperties_HasNullSpecialProperties()
        {
            var provider = new ReflectionMetadataProvider(typeof(SimpleCommandWithNoSpecialProps));

            // SpecialProperties should be null or have all null setters
            if (provider.SpecialProperties != null)
            {
                Assert.Null(provider.SpecialProperties.ParentSetter);
                Assert.Null(provider.SpecialProperties.SubcommandSetter);
                Assert.Null(provider.SpecialProperties.RemainingArgumentsSetter);
            }
        }

        [Command(Name = "simple")]
        private class SimpleCommandWithNoSpecialProps
        {
            [Option("-n|--name")]
            public string? Name { get; set; }
        }

        [Fact]
        public void ReflectionMetadataProvider_CommandWithoutCommandAttribute_HasNullCommandInfo()
        {
            var provider = new ReflectionMetadataProvider(typeof(ClassWithoutCommandAttribute));

            Assert.Null(provider.CommandInfo);
        }

        private class ClassWithoutCommandAttribute
        {
            public string? Value { get; set; }
        }

        [Fact]
        public void ReflectionMetadataProvider_CommandWithOnlyHelpOption_ExtractsHelpOption()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithOnlyHelpOption));

            Assert.NotNull(provider.HelpOption);
            Assert.Equal("-?|-h|--help", provider.HelpOption!.Template);
        }

        [Command(Name = "help-only")]
        [HelpOption("-?|-h|--help")]
        private class CommandWithOnlyHelpOption
        {
        }

        [Fact]
        public void ReflectionMetadataProvider_HelpOption_ExtractsInheritedFlag()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithInheritedHelpOption));

            Assert.NotNull(provider.HelpOption);
            Assert.True(provider.HelpOption!.Inherited);
        }

        [Command(Name = "inherited-help")]
        [HelpOption(Inherited = true)]
        private class CommandWithInheritedHelpOption
        {
        }

        [Fact]
        public void ReflectionMetadataProvider_CommandWithAllSpecialProperties_ExtractsAll()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithAllSpecialProps));

            Assert.NotNull(provider.SpecialProperties);
            Assert.NotNull(provider.SpecialProperties!.ParentSetter);
            Assert.Equal(typeof(ParentForAllSpecialProps), provider.SpecialProperties.ParentType);
            Assert.NotNull(provider.SpecialProperties.SubcommandSetter);
            Assert.Equal(typeof(object), provider.SpecialProperties.SubcommandType);
            Assert.NotNull(provider.SpecialProperties.RemainingArgumentsSetter);
            Assert.Equal(typeof(string[]), provider.SpecialProperties.RemainingArgumentsType);
        }

        [Command(Name = "parent-all")]
        [Subcommand(typeof(CommandWithAllSpecialProps))]
        private class ParentForAllSpecialProps
        {
        }

        [Command(Name = "all-special", UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue)]
        private class CommandWithAllSpecialProps
        {
            public ParentForAllSpecialProps? Parent { get; set; }
            public object? Subcommand { get; set; }
            public string[]? RemainingArguments { get; set; }
        }

        [Fact]
        public void ReflectionMetadataProvider_RemainingArguments_WithListType_ExtractsCorrectly()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithRemainingArgsList));

            Assert.NotNull(provider.SpecialProperties);
            Assert.NotNull(provider.SpecialProperties!.RemainingArgumentsSetter);
            Assert.Equal(typeof(List<string>), provider.SpecialProperties.RemainingArgumentsType);
        }

        [Command(Name = "remaining-list", UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue)]
        private class CommandWithRemainingArgsList
        {
            public List<string>? RemainingArguments { get; set; }
        }

        #endregion

        #region VersionOptionFromMember Tests

        [Fact]
        public void ReflectionMetadataProvider_VersionOptionFromMember_WithProperty_ExtractsGetter()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithVersionProperty));

            Assert.NotNull(provider.VersionOption);
            Assert.NotNull(provider.VersionOption!.VersionGetter);

            var instance = new CommandWithVersionProperty();
            var version = provider.VersionOption.VersionGetter!(instance);
            Assert.Equal("1.2.3-prop", version);
        }

        [Command(Name = "ver-prop")]
        [VersionOptionFromMember(MemberName = nameof(CommandWithVersionProperty.AppVersion))]
        private class CommandWithVersionProperty
        {
            public string AppVersion => "1.2.3-prop";
        }

        [Fact]
        public void ReflectionMetadataProvider_VersionOptionFromMember_WithMethod_ExtractsGetter()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithVersionMethod));

            Assert.NotNull(provider.VersionOption);
            Assert.NotNull(provider.VersionOption!.VersionGetter);

            var instance = new CommandWithVersionMethod();
            var version = provider.VersionOption.VersionGetter!(instance);
            Assert.Equal("2.0.0-method", version);
        }

        [Command(Name = "ver-method")]
        [VersionOptionFromMember(MemberName = nameof(CommandWithVersionMethod.GetVersion))]
        private class CommandWithVersionMethod
        {
            public string GetVersion() => "2.0.0-method";
        }

        [Fact]
        public void ReflectionMetadataProvider_VersionOptionFromMember_WithTemplate_ExtractsBoth()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithVersionTemplate));

            Assert.NotNull(provider.VersionOption);
            Assert.Equal("-V|--ver", provider.VersionOption!.Template);
            Assert.NotNull(provider.VersionOption.VersionGetter);

            var instance = new CommandWithVersionTemplate();
            var version = provider.VersionOption.VersionGetter!(instance);
            Assert.Equal("3.0.0", version);
        }

        [Command(Name = "ver-template")]
        [VersionOptionFromMember("-V|--ver", MemberName = nameof(Ver))]
        private class CommandWithVersionTemplate
        {
            public string Ver => "3.0.0";
        }

        [Fact]
        public void ReflectionMetadataProvider_VersionOptionFromMember_WithDescription_ExtractsAll()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithVersionDescription));

            Assert.NotNull(provider.VersionOption);
            Assert.Equal("Show version", provider.VersionOption!.Description);
        }

        [Command(Name = "ver-desc")]
        [VersionOptionFromMember(MemberName = nameof(Version), Description = "Show version")]
        private class CommandWithVersionDescription
        {
            public string Version => "4.0.0";
        }

        #endregion

        #region Constructor Injection Tests (Factory Creation)

        [Fact]
        public void ReflectionMetadataProvider_ModelFactory_CreatesInstanceWithParameterlessConstructor()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithParameterlessConstructor));
            var factory = provider.GetModelFactory(null);

            var instance = factory.Create();

            Assert.NotNull(instance);
            Assert.IsType<CommandWithParameterlessConstructor>(instance);
        }

        [Command(Name = "parameterless")]
        private class CommandWithParameterlessConstructor
        {
            public bool WasCreated { get; } = true;
        }

        [Fact]
        public void ReflectionMetadataProvider_ModelFactory_UsesServiceProvider()
        {
            var services = new TestServiceProvider();
            var testService = new TestServiceImpl();
            services.Register<ITestServiceForDI>(testService);

            var provider = new ReflectionMetadataProvider(typeof(CommandWithServiceDependency));
            var factory = provider.GetModelFactory(services);

            var instance = (CommandWithServiceDependency)factory.Create();

            Assert.NotNull(instance.Service);
            Assert.Same(testService, instance.Service);
        }

        public interface ITestServiceForDI
        {
            string GetValue();
        }

        private class TestServiceImpl : ITestServiceForDI
        {
            public string GetValue() => "test-value";
        }

        [Command(Name = "with-service")]
        private class CommandWithServiceDependency
        {
            public ITestServiceForDI? Service { get; }

            public CommandWithServiceDependency(ITestServiceForDI service)
            {
                Service = service;
            }
        }

        [Fact]
        public void ReflectionMetadataProvider_ModelFactory_FallsBackToParameterless_WhenServiceNotAvailable()
        {
            var services = new TestServiceProvider();
            // Not registering the service

            var provider = new ReflectionMetadataProvider(typeof(CommandWithOptionalService));
            var factory = provider.GetModelFactory(services);

            var instance = (CommandWithOptionalService)factory.Create();

            Assert.True(instance.UsedParameterlessConstructor);
        }

        [Command(Name = "optional-service")]
        private class CommandWithOptionalService
        {
            public bool UsedParameterlessConstructor { get; }

            public CommandWithOptionalService()
            {
                UsedParameterlessConstructor = true;
            }

            public CommandWithOptionalService(ITestServiceForDI service)
            {
                UsedParameterlessConstructor = false;
            }
        }

        private class TestServiceProvider : IServiceProvider
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

        #endregion

        #region Execute Handler Tests

        [Fact]
        public void ReflectionMetadataProvider_ExtractsOnExecute_SyncMethod()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithSyncExecute));

            Assert.NotNull(provider.ExecuteHandler);
            Assert.False(provider.ExecuteHandler!.IsAsync);
        }

        [Command(Name = "sync-exec")]
        private class CommandWithSyncExecute
        {
            public bool Executed { get; private set; }

            internal int OnExecute()
            {
                Executed = true;
                return 42;
            }
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsOnExecuteAsync_AsyncMethod()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithAsyncExecute));

            Assert.NotNull(provider.ExecuteHandler);
            Assert.True(provider.ExecuteHandler!.IsAsync);
        }

        [Command(Name = "async-exec")]
        private class CommandWithAsyncExecute
        {
            internal Task<int> OnExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task ReflectionMetadataProvider_ExecuteHandler_InvokesMethod()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithSyncExecute));
            var instance = new CommandWithSyncExecute();
            var app = new CommandLineApplication();

            var result = await provider.ExecuteHandler!.InvokeAsync(instance, app, CancellationToken.None);

            Assert.True(instance.Executed);
            Assert.Equal(42, result);
        }

        [Fact]
        public void ReflectionMetadataProvider_ExecuteHandler_WithAppParameter()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithAppParameter));

            Assert.NotNull(provider.ExecuteHandler);
        }

        [Command(Name = "app-param")]
        private class CommandWithAppParameter
        {
            public CommandLineApplication? ReceivedApp { get; private set; }

            internal int OnExecute(CommandLineApplication app)
            {
                ReceivedApp = app;
                return 0;
            }
        }

        [Fact]
        public async Task ReflectionMetadataProvider_ExecuteHandler_PassesAppParameter()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithAppParameter));
            var instance = new CommandWithAppParameter();
            var app = new CommandLineApplication();

            await provider.ExecuteHandler!.InvokeAsync(instance, app, CancellationToken.None);

            Assert.Same(app, instance.ReceivedApp);
        }

        #endregion

        #region Integration Tests - End-to-End Behavior

        [Fact]
        public void Integration_ParentChildRelationship_WorksEndToEnd()
        {
            var app = new CommandLineApplication<IntegrationParent>();
            app.Conventions.UseDefaultConventions();

            app.Parse("child", "-m", "hello");

            Assert.NotNull(app.Model.Subcommand);
            var child = Assert.IsType<IntegrationChild>(app.Model.Subcommand);
            Assert.Same(app.Model, child.Parent);
            Assert.Equal("hello", child.Message);
        }

        [Command(Name = "parent")]
        [Subcommand(typeof(IntegrationChild))]
        private class IntegrationParent
        {
            public object? Subcommand { get; set; }
        }

        [Command(Name = "child")]
        private class IntegrationChild
        {
            public IntegrationParent? Parent { get; set; }

            [Option("-m|--message")]
            public string? Message { get; set; }
        }

        [Fact]
        public void Integration_RemainingArguments_CollectedCorrectly()
        {
            var app = new CommandLineApplication<IntegrationRemainingArgs>();
            app.Conventions.UseDefaultConventions();

            app.Parse("arg1", "arg2", "arg3");

            Assert.NotNull(app.Model.RemainingArguments);
            Assert.Equal(new[] { "arg1", "arg2", "arg3" }, app.Model.RemainingArguments);
        }

        [Command(Name = "remaining", UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue)]
        private class IntegrationRemainingArgs
        {
            public string[]? RemainingArguments { get; set; }
        }

        [Fact]
        public void Integration_HelpOption_AppliedCorrectly()
        {
            var app = new CommandLineApplication<IntegrationHelpCommand>();
            app.Conventions.UseDefaultConventions();

            Assert.NotNull(app.OptionHelp);
            Assert.Equal("h", app.OptionHelp.ShortName);
            Assert.Equal("help", app.OptionHelp.LongName);
        }

        [Command(Name = "help-cmd")]
        [HelpOption("-h|--help", Description = "Get help")]
        private class IntegrationHelpCommand
        {
        }

        [Fact]
        public void Integration_VersionOption_AppliedCorrectly()
        {
            var app = new CommandLineApplication<IntegrationVersionCommand>();
            app.Conventions.UseDefaultConventions();

            Assert.NotNull(app.OptionVersion);
            Assert.Equal("v", app.OptionVersion.ShortName);
        }

        [Command(Name = "version-cmd")]
        [VersionOption("-v|--version", "1.0.0")]
        private class IntegrationVersionCommand
        {
        }

        [Fact]
        public void Integration_VersionOptionFromMember_AppliedCorrectly()
        {
            var app = new CommandLineApplication<IntegrationVersionFromMember>();
            app.Conventions.UseDefaultConventions();

            Assert.NotNull(app.OptionVersion);
        }

        [Command(Name = "version-member")]
        [VersionOptionFromMember(MemberName = nameof(Version))]
        private class IntegrationVersionFromMember
        {
            public string Version => "dynamic-version";
        }

        [Fact]
        public void Integration_ConstructorInjection_WorksWithConventions()
        {
            var services = new TestServiceProvider();
            var testService = new TestServiceImpl();
            services.Register<ITestServiceForDI>(testService);

            var app = new CommandLineApplication<CommandRequiringService>();
            app.Conventions.UseConstructorInjection(services);

            Assert.NotNull(app.Model);
            Assert.Same(testService, app.Model.Service);
        }

        [Command(Name = "require-service")]
        private class CommandRequiringService
        {
            public ITestServiceForDI Service { get; }

            public CommandRequiringService(ITestServiceForDI service)
            {
                Service = service;
            }
        }

        [Fact]
        public void Integration_CommandNameInference_WorksForAllPatterns()
        {
            var app = new CommandLineApplication<NameInferenceHost>();
            app.Conventions.UseDefaultConventions();

            // Check all inferred names
            Assert.NotNull(app.Commands.FirstOrDefault(c => c.Name == "simple"));
            Assert.NotNull(app.Commands.FirstOrDefault(c => c.Name == "my-complex-name"));
            Assert.NotNull(app.Commands.FirstOrDefault(c => c.Name == "add-user"));
            Assert.NotNull(app.Commands.FirstOrDefault(c => c.Name == "explicit-name"));
        }

        [Command(Name = "host")]
        [Subcommand(typeof(SimpleCommand), typeof(MyComplexNameCommand), typeof(AddUserCommand), typeof(ExplicitlyNamedCommand))]
        private class NameInferenceHost
        {
        }

        [Command]
        private class SimpleCommand
        {
        }

        [Command]
        private class MyComplexNameCommand
        {
        }

        [Command]
        private class AddUserCommand
        {
        }

        [Command(Name = "explicit-name")]
        private class ExplicitlyNamedCommand
        {
        }

        #endregion

        #region Validation Handler Tests

        [Fact]
        public void ReflectionMetadataProvider_ExtractsOnValidate_WhenPresent()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithValidation));

            Assert.NotNull(provider.ValidateHandler);
        }

        [Command(Name = "with-validation")]
        private class CommandWithValidation
        {
            [Option("-n|--number")]
            public int Number { get; set; }

            internal ValidationResult OnValidate()
            {
                if (Number < 0)
                {
                    return new ValidationResult("Number must be non-negative");
                }
                return ValidationResult.Success!;
            }
        }

        [Fact]
        public void ReflectionMetadataProvider_ValidateHandler_IsNull_WhenNoValidation()
        {
            var provider = new ReflectionMetadataProvider(typeof(SimpleCommandWithNoSpecialProps));

            Assert.Null(provider.ValidateHandler);
        }

        #endregion

        #region Subcommand Metadata Tests

        [Fact]
        public void ReflectionMetadataProvider_ExtractsMultipleSubcommands()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithMultipleSubcommands));

            Assert.Equal(3, provider.Subcommands.Count);
            Assert.Contains(provider.Subcommands, s => s.SubcommandType == typeof(Sub1));
            Assert.Contains(provider.Subcommands, s => s.SubcommandType == typeof(Sub2));
            Assert.Contains(provider.Subcommands, s => s.SubcommandType == typeof(Sub3));
        }

        [Command(Name = "multi-sub")]
        [Subcommand(typeof(Sub1), typeof(Sub2), typeof(Sub3))]
        private class CommandWithMultipleSubcommands
        {
        }

        [Command(Name = "sub1")]
        private class Sub1 { }

        [Command(Name = "sub2")]
        private class Sub2 { }

        [Command(Name = "sub3")]
        private class Sub3 { }

        [Fact]
        public void ReflectionMetadataProvider_SubcommandOrder_IsPreserved()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithMultipleSubcommands));

            var types = provider.Subcommands.Select(s => s.SubcommandType).ToArray();
            Assert.Equal(new[] { typeof(Sub1), typeof(Sub2), typeof(Sub3) }, types);
        }

        #endregion

        #region Option and Argument Metadata Tests

        [Fact]
        public void ReflectionMetadataProvider_ExtractsAllOptionProperties()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithDetailedOption));

            var option = provider.Options.FirstOrDefault(o => o.PropertyName == "Value");
            Assert.NotNull(option);
            Assert.Equal("-v|--value", option!.Template);
            Assert.Equal("A value", option.Description);
        }

        [Command(Name = "detailed-option")]
        private class CommandWithDetailedOption
        {
            [Option("-v|--value", Description = "A value")]
            public string? Value { get; set; }
        }

        [Fact]
        public void ReflectionMetadataProvider_ExtractsArgumentOrder()
        {
            var provider = new ReflectionMetadataProvider(typeof(CommandWithMultipleArguments));

            Assert.Equal(3, provider.Arguments.Count);
            Assert.Equal("First", provider.Arguments[0].PropertyName);
            Assert.Equal("Second", provider.Arguments[1].PropertyName);
            Assert.Equal("Third", provider.Arguments[2].PropertyName);
        }

        [Command(Name = "multi-arg")]
        private class CommandWithMultipleArguments
        {
            [Argument(0)]
            public string? First { get; set; }

            [Argument(1)]
            public string? Second { get; set; }

            [Argument(2)]
            public string? Third { get; set; }
        }

        #endregion
    }
}
