// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class TypedWrapperTests
    {
        [Command(Name = "test")]
        private class TestCommand
        {
            [Option("-n|--name")]
            public string? Name { get; set; }
        }

        [Fact]
        public void TypedMetadataProviderWrapper_DelegatesToInner()
        {
            var inner = new ReflectionMetadataProvider(typeof(TestCommand));
            var wrapper = new TypedMetadataProviderWrapper<TestCommand>(inner);

            Assert.Equal(inner.ModelType, wrapper.ModelType);
            Assert.Same(inner.Options, wrapper.Options);
            Assert.Same(inner.Arguments, wrapper.Arguments);
            Assert.Same(inner.Subcommands, wrapper.Subcommands);
            Assert.Same(inner.CommandInfo, wrapper.CommandInfo);
            Assert.Same(inner.ExecuteHandler, wrapper.ExecuteHandler);
            Assert.Same(inner.ValidateHandler, wrapper.ValidateHandler);
            Assert.Same(inner.ValidationErrorHandler, wrapper.ValidationErrorHandler);
            Assert.Same(inner.SpecialProperties, wrapper.SpecialProperties);
            Assert.Same(inner.HelpOption, wrapper.HelpOption);
            Assert.Same(inner.VersionOption, wrapper.VersionOption);
        }

        [Fact]
        public void TypedMetadataProviderWrapper_GetModelFactory_ReturnsTypedFactory()
        {
            var inner = new ReflectionMetadataProvider(typeof(TestCommand));
            var wrapper = new TypedMetadataProviderWrapper<TestCommand>(inner);

            var factory = wrapper.GetModelFactory(null);

            Assert.IsAssignableFrom<IModelFactory<TestCommand>>(factory);

            var instance = factory.Create();
            Assert.IsType<TestCommand>(instance);
        }

        [Fact]
        public void TypedMetadataProviderWrapper_ImplementsGenericInterface()
        {
            var inner = new ReflectionMetadataProvider(typeof(TestCommand));
            var wrapper = new TypedMetadataProviderWrapper<TestCommand>(inner);

            Assert.IsAssignableFrom<ICommandMetadataProvider<TestCommand>>(wrapper);
        }

        [Fact]
        public void TypedMetadataProviderWrapper_UntypedGetModelFactory_Works()
        {
            var inner = new ReflectionMetadataProvider(typeof(TestCommand));
            var wrapper = new TypedMetadataProviderWrapper<TestCommand>(inner);

            ICommandMetadataProvider untypedWrapper = wrapper;
            var factory = untypedWrapper.GetModelFactory(null);

            Assert.NotNull(factory);
            var instance = factory.Create();
            Assert.IsType<TestCommand>(instance);
        }

        [Fact]
        public void TypedModelFactoryWrapper_Create_ReturnsTypedInstance()
        {
            var inner = new ActivatorModelFactory(typeof(TestCommand));
            var wrapper = new TypedModelFactoryWrapper<TestCommand>(inner);

            TestCommand instance = wrapper.Create();

            Assert.NotNull(instance);
        }

        [Fact]
        public void TypedModelFactoryWrapper_UntypedCreate_Works()
        {
            var inner = new ActivatorModelFactory(typeof(TestCommand));
            var wrapper = new TypedModelFactoryWrapper<TestCommand>(inner);

            IModelFactory untypedWrapper = wrapper;
            var instance = untypedWrapper.Create();

            Assert.IsType<TestCommand>(instance);
        }

        [Fact]
        public void TypedModelFactoryWrapper_ImplementsBothInterfaces()
        {
            var inner = new ActivatorModelFactory(typeof(TestCommand));
            var wrapper = new TypedModelFactoryWrapper<TestCommand>(inner);

            Assert.IsAssignableFrom<IModelFactory>(wrapper);
            Assert.IsAssignableFrom<IModelFactory<TestCommand>>(wrapper);
        }
    }
}
