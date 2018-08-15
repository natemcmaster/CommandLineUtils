// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class SubcommandPropertyConventionTests
    {
        [Subcommand(typeof(AddCommand))]
        private class Program
        {
            public object Subcommand { get; set; }
        }

        private class AddCommand
        {
            public object Parent { get; }
        }

        [Fact]
        public void BindsToSubcommandProperty()
        {
            var app = new CommandLineApplication<Program>();
            app.Conventions.UseDefaultConventions();
            app.Parse("add");
            Assert.IsType<AddCommand>(app.Model.Subcommand);
        }
    }
}
