// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils.Conventions;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ConventionTestBase
    {
        protected readonly ITestOutputHelper _output;

        protected ConventionTestBase(ITestOutputHelper output)
        {
            _output = output;
        }

        protected CommandLineApplication<T> Create<T, TConvention>()
            where T : class
            where TConvention : IConvention, new()
        {
            var app = new CommandLineApplication<T>(new TestConsole(_output));
            app.Conventions.AddConvention(new TConvention());
            return app;
        }
    }
}
