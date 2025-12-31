// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class CommandLineApplicationWithModelTests
    {
        private class TestModel { }

        private class MockModelFactory : IModelFactory
        {
            private readonly object _model;
            public int CreateCallCount { get; private set; }

            public MockModelFactory(object model)
            {
                _model = model;
            }

            public object Create()
            {
                CreateCallCount++;
                return _model;
            }
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            var parent = new CommandLineApplication();
            var model = new TestModel();
            var factory = new MockModelFactory(model);

            var app = new CommandLineApplicationWithModel(parent, "test", typeof(TestModel), factory);

            Assert.Equal("test", app.Name);
            Assert.Same(parent, app.Parent);
        }

        [Fact]
        public void Model_ReturnsCreatedInstance()
        {
            var parent = new CommandLineApplication();
            var model = new TestModel();
            var factory = new MockModelFactory(model);
            var app = new CommandLineApplicationWithModel(parent, "test", typeof(TestModel), factory);

            var result = app.Model;

            Assert.Same(model, result);
        }

        [Fact]
        public void Model_IsLazyInitialized()
        {
            var parent = new CommandLineApplication();
            var model = new TestModel();
            var factory = new MockModelFactory(model);
            var app = new CommandLineApplicationWithModel(parent, "test", typeof(TestModel), factory);

            // Factory should not be called until Model is accessed
            Assert.Equal(0, factory.CreateCallCount);

            _ = app.Model;

            Assert.Equal(1, factory.CreateCallCount);
        }

        [Fact]
        public void Model_CachesInstance()
        {
            var parent = new CommandLineApplication();
            var model = new TestModel();
            var factory = new MockModelFactory(model);
            var app = new CommandLineApplicationWithModel(parent, "test", typeof(TestModel), factory);

            var first = app.Model;
            var second = app.Model;

            Assert.Same(first, second);
            Assert.Equal(1, factory.CreateCallCount);
        }

        [Fact]
        public void IModelAccessor_GetModelType_ReturnsModelType()
        {
            var parent = new CommandLineApplication();
            var model = new TestModel();
            var factory = new MockModelFactory(model);
            var app = new CommandLineApplicationWithModel(parent, "test", typeof(TestModel), factory);

            var accessor = (IModelAccessor)app;

            Assert.Equal(typeof(TestModel), accessor.GetModelType());
        }

        [Fact]
        public void IModelAccessor_GetModel_ReturnsModel()
        {
            var parent = new CommandLineApplication();
            var model = new TestModel();
            var factory = new MockModelFactory(model);
            var app = new CommandLineApplicationWithModel(parent, "test", typeof(TestModel), factory);

            var accessor = (IModelAccessor)app;

            Assert.Same(model, accessor.GetModel());
        }
    }
}
