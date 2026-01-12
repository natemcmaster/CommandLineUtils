// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    /// <summary>
    /// Tests that simulate patterns used in generated model factories.
    /// These tests verify the correctness of code generation patterns.
    /// </summary>
    public class GeneratedModelFactoryTests
    {
        #region Test Models

        private class ModelWithValueTypeConstructorParameters
        {
            public int Port { get; }
            public bool Verbose { get; }

            public ModelWithValueTypeConstructorParameters(int port, bool verbose)
            {
                Port = port;
                Verbose = verbose;
            }
        }

        private class ModelWithMixedConstructorParameters
        {
            public string Name { get; }
            public int Count { get; }

            public ModelWithMixedConstructorParameters(string name, int count)
            {
                Name = name;
                Count = count;
            }
        }

        #endregion

        #region Test Service Provider

        private class TestServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object?> _services = new();

            public void Register<T>(T service)
            {
                _services[typeof(T)] = service;
            }

            public object? GetService(Type serviceType)
            {
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        #endregion

        #region Bug Documentation: 'as' operator with value types

        /// <summary>
        /// Documents the bug in generated code: using 'as' with non-nullable value types doesn't compile.
        ///
        /// The current generator produces:
        ///   var p0_0 = _services.GetService(typeof(int)) as int;
        ///
        /// This is invalid C# because 'as' can only be used with reference types or nullable value types.
        /// The generated code will FAIL TO COMPILE for any command with value type constructor parameters.
        /// </summary>
        [Fact]
        public void Bug_AsOperatorWithNonNullableValueType_DoesNotCompile()
        {
            // This test documents the compilation failure.
            // We can't write code that fails to compile in a test, so we demonstrate
            // the issue conceptually:

            var services = new TestServiceProvider();
            services.Register<int>(8080);

            var service = services.GetService(typeof(int));

            // THIS IS THE BUG: The generator produces this pattern which doesn't compile:
            // var p0_0 = service as int;  // ERROR CS0077: The 'as' operator must be used with a reference type or nullable value type

            // To make it compile, we'd need to use nullable:
            var p0_0_nullable = service as int?;  // This compiles but defeats the purpose

            // Or better yet, use the correct pattern (check then cast):
            var p0_0_correct = service != null ? (int)service : default;

            Assert.NotNull(p0_0_nullable);
            Assert.Equal(8080, p0_0_nullable.Value);
            Assert.Equal(8080, p0_0_correct);
        }

        #endregion

        #region Correct Pattern: Check service then cast

        /// <summary>
        /// This factory simulates the CORRECT pattern that the generator should produce:
        /// check if service exists, then cast using direct cast operator.
        /// </summary>
        private class CorrectGeneratedFactory_CheckThenCast : IModelFactory<ModelWithValueTypeConstructorParameters>
        {
            private readonly IServiceProvider? _services;

            public CorrectGeneratedFactory_CheckThenCast(IServiceProvider? services) => _services = services;

            public ModelWithValueTypeConstructorParameters Create()
            {
                if (_services != null)
                {
                    var instance = _services.GetService(typeof(ModelWithValueTypeConstructorParameters)) as ModelWithValueTypeConstructorParameters;
                    if (instance != null) return instance;

                    // CORRECT PATTERN: Check service object, then cast
                    var service0_0 = _services.GetService(typeof(int));
                    var service0_1 = _services.GetService(typeof(bool));

                    if (service0_0 != null && service0_1 != null)
                    {
                        return new ModelWithValueTypeConstructorParameters((int)service0_0, (bool)service0_1);
                    }
                }

                throw new InvalidOperationException("Unable to create instance of ModelWithValueTypeConstructorParameters");
            }

            object IModelFactory.Create() => Create();
        }

        [Fact]
        public void CorrectPattern_CheckThenCast_WorksForValueTypeConstructorParameters()
        {
            // Arrange
            var services = new TestServiceProvider();
            services.Register<int>(8080);
            services.Register<bool>(true);

            var factory = new CorrectGeneratedFactory_CheckThenCast(services);

            // Act
            var instance = factory.Create();

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(8080, instance.Port);
            Assert.True(instance.Verbose);
        }

        [Fact]
        public void CorrectPattern_CheckThenCast_WorksForMixedReferenceAndValueTypes()
        {
            // Arrange
            var services = new TestServiceProvider();
            services.Register<string>("test-name");
            services.Register<int>(42);

            var factory = new CorrectGeneratedFactory_MixedTypes(services);

            // Act
            var instance = factory.Create();

            // Assert
            Assert.NotNull(instance);
            Assert.Equal("test-name", instance.Name);
            Assert.Equal(42, instance.Count);
        }

        private class CorrectGeneratedFactory_MixedTypes : IModelFactory<ModelWithMixedConstructorParameters>
        {
            private readonly IServiceProvider? _services;

            public CorrectGeneratedFactory_MixedTypes(IServiceProvider? services) => _services = services;

            public ModelWithMixedConstructorParameters Create()
            {
                if (_services != null)
                {
                    var instance = _services.GetService(typeof(ModelWithMixedConstructorParameters)) as ModelWithMixedConstructorParameters;
                    if (instance != null) return instance;

                    var service0_0 = _services.GetService(typeof(string));
                    var service0_1 = _services.GetService(typeof(int));

                    if (service0_0 != null && service0_1 != null)
                    {
                        return new ModelWithMixedConstructorParameters((string)service0_0, (int)service0_1);
                    }
                }

                throw new InvalidOperationException("Unable to create instance");
            }

            object IModelFactory.Create() => Create();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void CorrectPattern_FailsGracefully_WhenServiceNotRegistered()
        {
            // Arrange: Don't register the services
            var services = new TestServiceProvider();

            var factory = new CorrectGeneratedFactory_CheckThenCast(services);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => factory.Create());
            Assert.Contains("Unable to create instance", ex.Message);
        }

        [Fact]
        public void CorrectPattern_FailsGracefully_WhenPartialServicesRegistered()
        {
            // Arrange: Only register one of two required services
            var services = new TestServiceProvider();
            services.Register<int>(8080);
            // bool not registered

            var factory = new CorrectGeneratedFactory_CheckThenCast(services);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => factory.Create());
            Assert.Contains("Unable to create instance", ex.Message);
        }

        #endregion
    }
}
