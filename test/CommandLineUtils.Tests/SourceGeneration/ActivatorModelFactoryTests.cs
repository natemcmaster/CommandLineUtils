// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class ActivatorModelFactoryTests
    {
        private class SimpleModel
        {
            public string? Value { get; set; }
        }

        private class ModelWithDefaultValue
        {
            public string Name { get; set; } = "default";
        }

        private class ModelWithoutParameterlessConstructor
        {
            public ModelWithoutParameterlessConstructor(string required)
            {
                Required = required;
            }

            public string Required { get; }
        }

        private class TestServiceProvider : IServiceProvider
        {
            private readonly object? _service;
            private readonly Type? _serviceType;

            public TestServiceProvider(Type? serviceType = null, object? service = null)
            {
                _serviceType = serviceType;
                _service = service;
            }

            public object? GetService(Type serviceType)
            {
                if (_serviceType != null && serviceType == _serviceType)
                {
                    return _service;
                }
                return null;
            }
        }

        [Fact]
        public void Create_WithNoServices_UsesActivator()
        {
            var factory = new ActivatorModelFactory(typeof(SimpleModel));

            var instance = factory.Create();

            Assert.NotNull(instance);
            Assert.IsType<SimpleModel>(instance);
        }

        [Fact]
        public void Create_ReturnsNewInstanceEachTime()
        {
            var factory = new ActivatorModelFactory(typeof(SimpleModel));

            var instance1 = factory.Create();
            var instance2 = factory.Create();

            Assert.NotSame(instance1, instance2);
        }

        [Fact]
        public void Create_PreservesDefaultValues()
        {
            var factory = new ActivatorModelFactory(typeof(ModelWithDefaultValue));

            var instance = (ModelWithDefaultValue)factory.Create();

            Assert.Equal("default", instance.Name);
        }

        [Fact]
        public void Create_WithServices_UsesDI_WhenServiceExists()
        {
            var expectedModel = new SimpleModel { Value = "from DI" };
            var services = new TestServiceProvider(typeof(SimpleModel), expectedModel);
            var factory = new ActivatorModelFactory(typeof(SimpleModel), services);

            var instance = factory.Create();

            Assert.Same(expectedModel, instance);
        }

        [Fact]
        public void Create_WithServices_FallsBackToActivator_WhenServiceNotFound()
        {
            var services = new TestServiceProvider(); // No services registered
            var factory = new ActivatorModelFactory(typeof(SimpleModel), services);

            var instance = factory.Create();

            Assert.NotNull(instance);
            Assert.IsType<SimpleModel>(instance);
        }

        [Fact]
        public void Create_WithNullServices_UsesActivator()
        {
            var factory = new ActivatorModelFactory(typeof(SimpleModel), services: null);

            var instance = factory.Create();

            Assert.NotNull(instance);
            Assert.IsType<SimpleModel>(instance);
        }

        [Fact]
        public void Create_TypeWithoutParameterlessConstructor_ThrowsException()
        {
            var factory = new ActivatorModelFactory(typeof(ModelWithoutParameterlessConstructor));

            Assert.Throws<MissingMethodException>(() => factory.Create());
        }

        [Fact]
        public void Create_TypeWithoutParameterlessConstructor_WorksWithDI()
        {
            var expectedModel = new ModelWithoutParameterlessConstructor("from DI");
            var services = new TestServiceProvider(typeof(ModelWithoutParameterlessConstructor), expectedModel);
            var factory = new ActivatorModelFactory(typeof(ModelWithoutParameterlessConstructor), services);

            var instance = factory.Create();

            Assert.Same(expectedModel, instance);
            Assert.Equal("from DI", ((ModelWithoutParameterlessConstructor)instance).Required);
        }

        [Fact]
        public void Create_ImplementsIModelFactory()
        {
            var factory = new ActivatorModelFactory(typeof(SimpleModel));

            Assert.IsAssignableFrom<IModelFactory>(factory);
        }
    }
}
