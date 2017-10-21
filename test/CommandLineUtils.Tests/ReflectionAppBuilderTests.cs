// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ReflectionAppBuilderTests
    {
        private class AttributesNotUsedClass
        {
            public int OptionA { get; set; }
            public string OptionB { get; set; }
        }

        [Fact]
        public void AttributesAreRequired()
        {
            var builder = new ReflectionAppBuilder<AttributesNotUsedClass>();
            Assert.Empty(builder.App.Arguments);
            Assert.Empty(builder.App.Commands);
            Assert.Empty(builder.App.Options);
            Assert.Null(builder.App.OptionHelp);
            Assert.Null(builder.App.OptionVersion);
        }
    }
}
