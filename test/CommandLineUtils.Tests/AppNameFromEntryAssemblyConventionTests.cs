// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class AppNameFromEntryAssemblyConventionTests
    {
        [Fact]
        public void ItSetsAppNameToEntryAssemblyIfNotSpecified()
        {
            if (Assembly.GetEntryAssembly() == null)
            {
                return;
            }

            var expected = Assembly.GetEntryAssembly().GetName().Name;
            var app = new CommandLineApplication();
            app.Conventions.SetAppNameFromEntryAssembly();
            Assert.Equal(expected, app.Name);
        }
    }
}
