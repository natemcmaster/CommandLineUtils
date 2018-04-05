// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class PrefixRootFullNamePropertyConventionTests
    {
        [Subcommand("addVegetable", typeof(AddVegetable))]
        [Subcommand("addFruit", typeof(AddFruit))]
        [PrefixRootFullName]
        private class Program
        {
            [Subcommand("cucumber", typeof(Cucumber))]
            [Subcommand("onion", typeof(Onion))]
            private class AddVegetable
            {
                private class Cucumber
                {
                }

                private class Onion
                {
                }
            }

            [PrefixRootFullName(false)]
            [Subcommand("apple", typeof(Apple))]
            [Subcommand("pear", typeof(Pear))]
            private class AddFruit
            {
                private class Apple
                {
                }

                private class Pear
                {
                }
            }

        }

        [Fact]
        public void InheritanceAndAttribsByDefault()
        {
            var app = new CommandLineApplication<Program>();
            app.Conventions
                .UseDefaultConventions();
            var result1 = app.Parse("addVegetable onion".Split(' '));
            var result2 = app.Parse("addFruit apple".Split(' '));
            Assert.True(result1.SelectedCommand.PrefixRootFullName);
            Assert.False(result2.SelectedCommand.PrefixRootFullName);
        }

        [Fact]
        public void InheritanceAndAttribs()
        {
            var app = new CommandLineApplication<Program>();
            app.Conventions
                .SetSubcommandPropertyOnModel()
                .UseSubcommandAttributes()
                .UsePrefixRootFullName();
            var result1 = app.Parse("addVegetable onion".Split(' '));
            var result2 = app.Parse("addFruit apple".Split(' '));
            Assert.True(result1.SelectedCommand.PrefixRootFullName);
            Assert.False(result2.SelectedCommand.PrefixRootFullName);
        }

        [Fact]
        public void FeatureNotSet()
        {
            var app = new CommandLineApplication<Program>();
            app.Conventions
                .SetSubcommandPropertyOnModel()
                .UseSubcommandAttributes();
            var result1 = app.Parse("addVegetable onion".Split(' '));
            var result2 = app.Parse("addFruit apple".Split(' '));
            Assert.False(result1.SelectedCommand.PrefixRootFullName);
            Assert.False(result2.SelectedCommand.PrefixRootFullName);
        }

    }
}
