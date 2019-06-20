// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils.Conventions;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandNameFromTypeConventionTests
    {
        [Theory]
        [InlineData("Command", "command")]
        [InlineData("AddCommand", "add")]
        [InlineData("RemoveItem", "remove-item")]
        [InlineData("Rm_Item", "rm-item")]
        [InlineData("Rm_Item_Command", "rm-item")]
        public void ItInfersCommandName(string typeName, string commandName)
            => Assert.Equal(commandName, CommandNameFromTypeConvention.GetCommandName(typeName));

        [Subcommand(typeof(AddCommand))]
        private class Program
        { }

        private class AddCommand
        { }

        [Fact]
        public void ItInfersSubcommandNameFromTypeName()
        {
            var app = new CommandLineApplication<Program>();
            app.Conventions.UseDefaultConventions();
            var sub = Assert.Single(app.Commands);
            Assert.Equal("add", sub.Name);
        }
    }
}
