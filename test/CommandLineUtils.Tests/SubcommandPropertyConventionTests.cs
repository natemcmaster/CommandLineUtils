// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class SubcommandPropertyConventionTests
    {
        [Subcommand(typeof(AddCmd))]
        private class Program
        {
            public object Subcommand { get; set; }
        }

        [Command("add")]
        private class AddCmd
        {
            public object Parent { get; }
        }

        [Fact]
        public void BindsToSubcommandProperty()
        {
            var app = new CommandLineApplication<Program>();
            app.Conventions.SetSubcommandPropertyOnModel().UseSubcommandAttributes().UseCommandAttribute();
            app.Parse("add");
            Assert.IsType<AddCmd>(app.Model.Subcommand);
        }
    }
}
