// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class InjectedModelConventionTests
    {
        private readonly ITestOutputHelper _output;

        public InjectedModelConventionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ItInjectsModelFromDefaultServices()
        {
            var app = new CommandLineApplication<IConsole>();
            app.Conventions.UseInjectedModel();
            app.Parse();
            Assert.NotNull(app.Model);
            Assert.IsType<PhysicalConsole>(app.Model);
        }

        [Fact]
        public void ItSupportsCustomServices()
        {
            var testConsole = new TestConsole(_output);
            var services = new ServiceCollection()
                .AddSingleton<IConsole>(testConsole)
                .BuildServiceProvider();
            var app = new CommandLineApplication<IConsole>();
            app.Conventions.UseInjectedModel(services);
            Assert.Same(testConsole, app.Model);
        }

        private interface ITestApp
        {

        }

        private class TestApp : ITestApp
        {

        }

        [Fact]
        public void ItSupportsInterfaceModels()
        {
            var testApp = new TestApp();
            var services = new ServiceCollection()
                .AddSingleton<ITestApp>(testApp)
                .BuildServiceProvider();
            var app = new CommandLineApplication<ITestApp>();
            app.Conventions.UseInjectedModel(services);
            Assert.Same(testApp, app.Model);
        }

        [Fact]
        public void ThrowsWhenNoModelFound()
        {
            var app = new CommandLineApplication<ITestApp>();
            app.Conventions.UseInjectedModel();
            var ex = Assert.Throws<MissingParameterlessConstructorException>(() => app.Model);
            Assert.Equal(typeof(ITestApp), ex.Type);
        }
    }
}
