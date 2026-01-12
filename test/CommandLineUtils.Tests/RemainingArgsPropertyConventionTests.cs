// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils.Conventions;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class RemainingArgsPropertyConventionTests : ConventionTestBase
    {
        public RemainingArgsPropertyConventionTests(ITestOutputHelper output) : base(output)
        {
        }

        protected CommandLineApplication<T> Create<T>() where T : class
        {
            var app = Create<T, RemainingArgsPropertyConvention>();
            app.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect;
            return app;
        }

        private class RemainingArguments_Array
        {
            public string[]? RemainingArguments { get; }
        }

        [Fact]
        public void ItSetsRemainingArguments_Array()
        {
            var app = Create<RemainingArguments_Array>();
            app.Parse("a", "b");
            Assert.Equal(new[] { "a", "b" }, app.Model.RemainingArguments);
        }

        private class RemainingArgs_Array
        {
            public string[]? RemainingArgs { get; }
        }


        [Fact]
        public void ItSetsRemainingArgs_Array()
        {
            var app = Create<RemainingArgs_Array>();
            app.Parse("a", "b");
            Assert.Equal(new[] { "a", "b" }, app.Model.RemainingArgs);
        }

        private class Parent
        {
            public object? Subcommand { get; }
        }

        [Fact]
        public void ItSetsRemainingArgsOnSubcommand()
        {
            var app = Create<Parent>();
            app.Command<RemainingArgs_Array>("subcmd", s =>
            {
                s.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect;
            });
            var result = app.Parse("subcmd", "a", "b");
            var subcmd = Assert.IsType<CommandLineApplication<RemainingArgs_Array>>(result.SelectedCommand);
            Assert.Equal(new[] { "a", "b" }, subcmd.Model.RemainingArgs);
        }

        private class RemainingArgs_List
        {
            public List<string>? RemainingArguments { get; }
        }

        [Fact]
        public void ItSetsRemainingArguments_List()
        {
            var app = new CommandLineApplication<RemainingArgs_List>
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            };
            app.Conventions.UseDefaultConventions();
            app.Parse("a", "b");
            Assert.Equal(new[] { "a", "b" }, app.Model.RemainingArguments);
        }

        #region Reflection Fallback and Error Tests

        private class RemainingArgs_InvalidType
        {
            // This type is not assignable to IReadOnlyList<string>
            public int RemainingArguments { get; set; }
        }

        [Fact]
        public void ThrowsWhenPropertyTypeIsInvalid()
        {
            // This tests lines 57-59: InvalidOperationException when property type
            // is not assignable to IReadOnlyList<string>
            var app = new CommandLineApplication<RemainingArgs_InvalidType>
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            };

            var ex = Assert.Throws<InvalidOperationException>(() =>
                app.Conventions.AddConvention(new RemainingArgsPropertyConvention()));

            Assert.Contains("RemainingArgs", ex.Message);
        }

        private class RemainingArgs_IReadOnlyList
        {
            public IReadOnlyList<string>? RemainingArguments { get; set; }
        }

        [Fact]
        public void ItSetsRemainingArguments_IReadOnlyList()
        {
            // Tests the IReadOnlyList<string> path (lines 62-63)
            var app = Create<RemainingArgs_IReadOnlyList>();
            app.Parse("x", "y", "z");
            Assert.Equal(new[] { "x", "y", "z" }, app.Model.RemainingArguments);
        }

        private class RemainingArgs_IEnumerable
        {
            public IEnumerable<string>? RemainingArguments { get; set; }
        }

        [Fact]
        public void ThrowsWhenPropertyTypeIsIEnumerable()
        {
            // IEnumerable<string> is NOT valid because the convention assigns IReadOnlyList<string>
            // and IEnumerable<string> is not assignable from IReadOnlyList<string>
            var app = new CommandLineApplication<RemainingArgs_IEnumerable>
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            };

            var ex = Assert.Throws<InvalidOperationException>(() =>
                app.Conventions.AddConvention(new RemainingArgsPropertyConvention()));

            Assert.Contains("RemainingArgs", ex.Message);
        }

        private class NoRemainingArgsProperty
        {
            public string? SomeOtherProperty { get; set; }
        }

        [Fact]
        public void DoesNotThrowWhenNoRemainingArgsProperty()
        {
            // Tests lines 40-42: early return when no matching property
            var app = new CommandLineApplication<NoRemainingArgsProperty>
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            };

            // Should not throw, just silently skip
            app.Conventions.AddConvention(new RemainingArgsPropertyConvention());
            app.Parse("extra", "args");

            // No property to set, so nothing happens
            Assert.Null(app.Model.SomeOtherProperty);
        }

        /// <summary>
        /// Tests the reflection fallback path (lines 45-48) when no generated metadata is available.
        /// We need to ensure the registry is clear so no metadata is found.
        /// </summary>
        private class RemainingArgsReflectionFallback
        {
            public string[]? RemainingArguments { get; set; }
        }

        [Fact]
        public void UsesReflectionFallback_WhenNoGeneratedMetadata()
        {
            // Ensure no registered metadata
            CommandMetadataRegistry.Clear();

            var app = new CommandLineApplication<RemainingArgsReflectionFallback>
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            };

            // Apply convention directly - this uses reflection fallback since no metadata is registered
            app.Conventions.AddConvention(new RemainingArgsPropertyConvention());
            app.Parse("arg1", "arg2", "arg3");

            Assert.Equal(new[] { "arg1", "arg2", "arg3" }, app.Model.RemainingArguments);
        }

        private class RemainingArgsPrivateProperty
        {
            private string[]? RemainingArguments { get; set; }

            public string[]? GetRemainingArgs() => RemainingArguments;
        }

        [Fact]
        public void FindsPrivateRemainingArgsProperty()
        {
            // Tests that the property binding flags include NonPublic
            var app = Create<RemainingArgsPrivateProperty>();
            app.Parse("private", "args");
            Assert.Equal(new[] { "private", "args" }, app.Model.GetRemainingArgs());
        }

        private class RemainingArgs_StaticProperty
        {
            public static string[]? RemainingArguments { get; set; }
        }

        [Fact]
        public void FindsStaticRemainingArgsProperty()
        {
            // Reset static property
            RemainingArgs_StaticProperty.RemainingArguments = null;

            // Tests that the property binding flags include Static
            var app = Create<RemainingArgs_StaticProperty>();
            app.Parse("static", "args");
            Assert.Equal(new[] { "static", "args" }, RemainingArgs_StaticProperty.RemainingArguments);
        }

        #endregion
    }
}
