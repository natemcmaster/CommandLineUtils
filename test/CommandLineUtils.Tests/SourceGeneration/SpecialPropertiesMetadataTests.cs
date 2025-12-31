// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class SpecialPropertiesMetadataTests
    {
        private class ParentCommand
        {
            public string? ParentName { get; set; }
        }

        private class ChildCommand
        {
            public ParentCommand? Parent { get; set; }
            public object? Subcommand { get; set; }
            public string[]? RemainingArguments { get; set; }
        }

        [Fact]
        public void DefaultValues_AreNull()
        {
            var metadata = new SpecialPropertiesMetadata();

            Assert.Null(metadata.ParentSetter);
            Assert.Null(metadata.ParentType);
            Assert.Null(metadata.SubcommandSetter);
            Assert.Null(metadata.SubcommandType);
            Assert.Null(metadata.RemainingArgumentsSetter);
            Assert.Null(metadata.RemainingArgumentsType);
        }

        [Fact]
        public void ParentSetter_CanBeSetAndInvoked()
        {
            var metadata = new SpecialPropertiesMetadata
            {
                ParentSetter = (obj, val) => ((ChildCommand)obj).Parent = (ParentCommand?)val,
                ParentType = typeof(ParentCommand)
            };

            var child = new ChildCommand();
            var parent = new ParentCommand { ParentName = "TestParent" };

            metadata.ParentSetter!(child, parent);

            Assert.Same(parent, child.Parent);
            Assert.Equal(typeof(ParentCommand), metadata.ParentType);
        }

        [Fact]
        public void SubcommandSetter_CanBeSetAndInvoked()
        {
            var metadata = new SpecialPropertiesMetadata
            {
                SubcommandSetter = (obj, val) => ((ChildCommand)obj).Subcommand = val,
                SubcommandType = typeof(object)
            };

            var command = new ChildCommand();
            var subcommand = new object();

            metadata.SubcommandSetter!(command, subcommand);

            Assert.Same(subcommand, command.Subcommand);
            Assert.Equal(typeof(object), metadata.SubcommandType);
        }

        [Fact]
        public void RemainingArgumentsSetter_CanBeSetAndInvoked()
        {
            var metadata = new SpecialPropertiesMetadata
            {
                RemainingArgumentsSetter = (obj, val) => ((ChildCommand)obj).RemainingArguments = (string[]?)val,
                RemainingArgumentsType = typeof(string[])
            };

            var command = new ChildCommand();
            var args = new[] { "arg1", "arg2", "arg3" };

            metadata.RemainingArgumentsSetter!(command, args);

            Assert.Same(args, command.RemainingArguments);
            Assert.Equal(typeof(string[]), metadata.RemainingArgumentsType);
        }

        [Fact]
        public void AllProperties_CanBeSetTogether()
        {
            var metadata = new SpecialPropertiesMetadata
            {
                ParentSetter = (obj, val) => ((ChildCommand)obj).Parent = (ParentCommand?)val,
                ParentType = typeof(ParentCommand),
                SubcommandSetter = (obj, val) => ((ChildCommand)obj).Subcommand = val,
                SubcommandType = typeof(object),
                RemainingArgumentsSetter = (obj, val) => ((ChildCommand)obj).RemainingArguments = (string[]?)val,
                RemainingArgumentsType = typeof(string[])
            };

            var command = new ChildCommand();
            var parent = new ParentCommand();
            var subcommand = new object();
            var args = new[] { "arg1" };

            metadata.ParentSetter!(command, parent);
            metadata.SubcommandSetter!(command, subcommand);
            metadata.RemainingArgumentsSetter!(command, args);

            Assert.Same(parent, command.Parent);
            Assert.Same(subcommand, command.Subcommand);
            Assert.Same(args, command.RemainingArguments);
        }

        [Fact]
        public void ParentSetter_CanSetNull()
        {
            var metadata = new SpecialPropertiesMetadata
            {
                ParentSetter = (obj, val) => ((ChildCommand)obj).Parent = (ParentCommand?)val,
                ParentType = typeof(ParentCommand)
            };

            var child = new ChildCommand { Parent = new ParentCommand() };

            metadata.ParentSetter!(child, null);

            Assert.Null(child.Parent);
        }
    }
}
