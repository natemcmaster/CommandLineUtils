// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
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

        private class CommandWithNullableArrayRemainingArgs
        {
            public string[]? RemainingArguments { get; set; }
        }

        private class CommandWithNullableElementArrayRemainingArgs
        {
            public string?[]? RemainingArguments { get; set; }
        }

        private class CommandWithReadOnlyListRemainingArgs
        {
            public System.Collections.Generic.IReadOnlyList<string>? RemainingArguments { get; set; }
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

        #region RemainingArguments Array Conversion Tests

        [Fact]
        public void RemainingArgumentsSetter_ArrayType_ConvertsFromIReadOnlyList()
        {
            // This simulates the CORRECT generated code for array types:
            // They need conversion from IReadOnlyList to array
            var metadata = new SpecialPropertiesMetadata
            {
                RemainingArgumentsSetter = static (obj, val) =>
                    ((CommandWithNullableArrayRemainingArgs)obj).RemainingArguments =
                        val is string[] arr ? arr : ((System.Collections.Generic.IReadOnlyList<string>)val!).ToArray(),
                RemainingArgumentsType = typeof(string[])
            };

            var command = new CommandWithNullableArrayRemainingArgs();
            System.Collections.Generic.IReadOnlyList<string> input = new[] { "arg1", "arg2" };

            // Should convert IReadOnlyList to array
            metadata.RemainingArgumentsSetter!(command, input);

            Assert.NotNull(command.RemainingArguments);
            Assert.Equal(2, command.RemainingArguments.Length);
            Assert.Equal("arg1", command.RemainingArguments[0]);
            Assert.Equal("arg2", command.RemainingArguments[1]);
        }

        [Fact]
        public void RemainingArgumentsSetter_ArrayType_AcceptsArrayDirectly()
        {
            // Array types can also accept arrays directly without conversion
            var metadata = new SpecialPropertiesMetadata
            {
                RemainingArgumentsSetter = static (obj, val) =>
                    ((CommandWithNullableArrayRemainingArgs)obj).RemainingArguments =
                        val is string[] arr ? arr : ((System.Collections.Generic.IReadOnlyList<string>)val!).ToArray(),
                RemainingArgumentsType = typeof(string[])
            };

            var command = new CommandWithNullableArrayRemainingArgs();
            var input = new[] { "arg1", "arg2" };

            metadata.RemainingArgumentsSetter!(command, input);

            Assert.Same(input, command.RemainingArguments);
        }

        [Fact]
        public void RemainingArgumentsSetter_ReadOnlyListType_CastsDirectly()
        {
            // Non-array collection types should use direct cast (no conversion)
            var metadata = new SpecialPropertiesMetadata
            {
                RemainingArgumentsSetter = static (obj, val) =>
                    ((CommandWithReadOnlyListRemainingArgs)obj).RemainingArguments =
                        (System.Collections.Generic.IReadOnlyList<string>?)val,
                RemainingArgumentsType = typeof(System.Collections.Generic.IReadOnlyList<string>)
            };

            var command = new CommandWithReadOnlyListRemainingArgs();
            System.Collections.Generic.IReadOnlyList<string> input = new[] { "arg1", "arg2" };

            metadata.RemainingArgumentsSetter!(command, input);

            Assert.Same(input, command.RemainingArguments);
        }

        [Fact]
        public void RemainingArgumentsSetter_NullableElementArray_ConvertsFromIReadOnlyList()
        {
            // Nullable element arrays (string?[]) should still use array conversion
            var metadata = new SpecialPropertiesMetadata
            {
                RemainingArgumentsSetter = static (obj, val) =>
                    ((CommandWithNullableElementArrayRemainingArgs)obj).RemainingArguments =
                        val is string?[] arr ? arr : ((System.Collections.Generic.IReadOnlyList<string>)val!).ToArray(),
                RemainingArgumentsType = typeof(string?[])
            };

            var command = new CommandWithNullableElementArrayRemainingArgs();
            System.Collections.Generic.IReadOnlyList<string> input = new[] { "arg1", "arg2" };

            metadata.RemainingArgumentsSetter!(command, input);

            Assert.NotNull(command.RemainingArguments);
            Assert.Equal(2, command.RemainingArguments.Length);
        }

        [Fact]
        public void BugDemo_DirectCastFails_WhenIReadOnlyListPassedToArrayProperty()
        {
            // This demonstrates what happens with the BUGGY code when string comparison fails
            // It would try to cast IReadOnlyList directly to array, causing InvalidCastException

            System.Collections.Generic.IReadOnlyList<string> input = new[] { "arg1", "arg2" };

            // This simulates the buggy generated code for nullable arrays (string[]?)
            // when the string comparison fails: (string[]?)val
            Assert.Throws<InvalidCastException>(() =>
            {
                var _ = (string[]?)input; // Can't cast IReadOnlyList to array!
            });
        }

        #endregion
    }
}
