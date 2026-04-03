// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.Conventions
{
    /// <summary>
    /// Comprehensive tests for OptionAttributeConventionBase to achieve full code coverage.
    /// </summary>
    public class OptionAttributeConventionBaseTests
    {
        #region Test Helper Convention

        /// <summary>
        /// A test convention that exposes the protected AddOption method for testing.
        /// </summary>
        private class TestOptionConvention : OptionAttributeConventionBase<OptionAttribute>, IConvention
        {
            private readonly PropertyInfo _property;
            private readonly CommandOption _option;

            public TestOptionConvention(PropertyInfo property, CommandOption option)
            {
                _property = property;
                _option = option;
            }

            public void Apply(ConventionContext context)
            {
                AddOption(context, _option, _property);
            }

            public static void CallAddOption(ConventionContext context, CommandOption option, PropertyInfo prop)
            {
                var convention = new TestOptionConvention(prop, option);
                convention.AddOption(context, option, prop);
            }
        }

        #endregion

        #region Test Models

        private class ModelWithStringOption
        {
            public string? Value { get; set; }
        }

        private class ModelWithStringOptionWithDefault
        {
            public string Value { get; set; } = "default";
        }

        private class ModelWithIntOption
        {
            public int Count { get; set; }
        }

        private class ModelWithStringArrayOption
        {
            public string[]? Values { get; set; }
        }

        private class ModelWithStringArrayOptionWithDefault
        {
            public string[] Values { get; set; } = new[] { "a", "b" };
        }

        private class ModelWithBoolOption
        {
            public bool Flag { get; set; }
        }

        private class ModelWithBoolArrayOption
        {
            public bool[]? Flags { get; set; }
        }

        private class ModelWithNullableBoolOption
        {
            public bool? Flag { get; set; }
        }

        private class ModelWithValidation
        {
            [Required]
            public string? Name { get; set; }
        }

        private class ModelWithValueTuple
        {
            public (bool hasValue, string? value) Option { get; set; }
        }

        #endregion

        #region Null ModelAccessor Tests

        [Fact]
        public void AddOption_ThrowsWhenModelAccessorIsNull()
        {
            var app = new CommandLineApplication(); // Non-generic, no model
            var context = new ConventionContext(app, null);
            var prop = typeof(ModelWithStringOption).GetProperty(nameof(ModelWithStringOption.Value))!;
            var option = new CommandOption("-v|--value", CommandOptionType.SingleValue);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                TestOptionConvention.CallAddOption(context, option, prop));

            Assert.Equal(Strings.ConventionRequiresModel, ex.Message);
        }

        #endregion

        #region ValidationAttribute Tests

        [Fact]
        public void AddOption_AddsValidationAttributes()
        {
            var app = new CommandLineApplication<ModelWithValidation>();
            var context = new ConventionContext(app, typeof(ModelWithValidation));
            var prop = typeof(ModelWithValidation).GetProperty(nameof(ModelWithValidation.Name))!;
            var option = app.Option("-n|--name", "Name", CommandOptionType.SingleValue);

            TestOptionConvention.CallAddOption(context, option, prop);

            Assert.Single(option.Validators);
        }

        #endregion

        #region NoValue Option Type Tests

        [Fact]
        public void AddOption_NoValue_ThrowsForNonBooleanType()
        {
            var app = new CommandLineApplication<ModelWithStringOption>();
            var context = new ConventionContext(app, typeof(ModelWithStringOption));
            var prop = typeof(ModelWithStringOption).GetProperty(nameof(ModelWithStringOption.Value))!;
            var option = new CommandOption("-v|--value", CommandOptionType.NoValue);
            app.AddOption(option);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                TestOptionConvention.CallAddOption(context, option, prop));

            Assert.Equal(Strings.NoValueTypesMustBeBoolean, ex.Message);
        }

        [Fact]
        public void AddOption_NoValue_WorksWithBool()
        {
            var app = new CommandLineApplication<ModelWithBoolOption>();
            var context = new ConventionContext(app, typeof(ModelWithBoolOption));
            var prop = typeof(ModelWithBoolOption).GetProperty(nameof(ModelWithBoolOption.Flag))!;
            var option = app.Option("-f|--flag", "Flag", CommandOptionType.NoValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse("-f");

            Assert.True(app.Model.Flag);
        }

        [Fact]
        public void AddOption_NoValue_WorksWithNullableBool()
        {
            var app = new CommandLineApplication<ModelWithNullableBoolOption>();
            var context = new ConventionContext(app, typeof(ModelWithNullableBoolOption));
            var prop = typeof(ModelWithNullableBoolOption).GetProperty(nameof(ModelWithNullableBoolOption.Flag))!;
            var option = app.Option("-f|--flag", "Flag", CommandOptionType.NoValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse("-f");

            Assert.True(app.Model.Flag);
        }

        [Fact]
        public void AddOption_NoValue_WorksWithBoolArray()
        {
            var app = new CommandLineApplication<ModelWithBoolArrayOption>();
            var context = new ConventionContext(app, typeof(ModelWithBoolArrayOption));
            var prop = typeof(ModelWithBoolArrayOption).GetProperty(nameof(ModelWithBoolArrayOption.Flags))!;
            var option = app.Option("-f|--flag", "Flag", CommandOptionType.NoValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse("-f", "-f", "-f");

            Assert.NotNull(app.Model.Flags);
            Assert.Equal(3, app.Model.Flags!.Length);
            Assert.All(app.Model.Flags, f => Assert.True(f));
        }

        [Fact]
        public void AddOption_NoValue_BoolArrayWithNoValue_SetsEmptyArray()
        {
            var app = new CommandLineApplication<ModelWithBoolArrayOption>();
            var context = new ConventionContext(app, typeof(ModelWithBoolArrayOption));
            var prop = typeof(ModelWithBoolArrayOption).GetProperty(nameof(ModelWithBoolArrayOption.Flags))!;
            var option = app.Option("-f|--flag", "Flag", CommandOptionType.NoValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse(); // No flag provided

            // The model should have an empty array set
            Assert.NotNull(app.Model.Flags);
            Assert.Empty(app.Model.Flags!);
        }

        [Fact]
        public void AddOption_NoValue_BoolWithNoValue_DoesNotSetProperty()
        {
            var app = new CommandLineApplication<ModelWithBoolOption>();
            var context = new ConventionContext(app, typeof(ModelWithBoolOption));
            var prop = typeof(ModelWithBoolOption).GetProperty(nameof(ModelWithBoolOption.Flag))!;
            var option = app.Option("-f|--flag", "Flag", CommandOptionType.NoValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse(); // No flag provided

            Assert.False(app.Model.Flag);
        }

        #endregion

        #region SingleValue Option Type Tests

        [Fact]
        public void AddOption_SingleValue_ParsesValue()
        {
            var app = new CommandLineApplication<ModelWithStringOption>();
            var context = new ConventionContext(app, typeof(ModelWithStringOption));
            var prop = typeof(ModelWithStringOption).GetProperty(nameof(ModelWithStringOption.Value))!;
            var option = app.Option("-v|--value", "Value", CommandOptionType.SingleValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse("-v", "test");

            Assert.Equal("test", app.Model.Value);
        }

        [Fact]
        public void AddOption_SingleValue_UsesDefaultValue()
        {
            var app = new CommandLineApplication<ModelWithStringOptionWithDefault>();
            var context = new ConventionContext(app, typeof(ModelWithStringOptionWithDefault));
            var prop = typeof(ModelWithStringOptionWithDefault).GetProperty(nameof(ModelWithStringOptionWithDefault.Value))!;
            var option = app.Option("-v|--value", "Value", CommandOptionType.SingleValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse(); // No value provided

            Assert.Equal("default", app.Model.Value);
            Assert.Equal("default", option.DefaultValue);
        }

        [Fact]
        public void AddOption_SingleValue_ParsesIntValue()
        {
            var app = new CommandLineApplication<ModelWithIntOption>();
            var context = new ConventionContext(app, typeof(ModelWithIntOption));
            var prop = typeof(ModelWithIntOption).GetProperty(nameof(ModelWithIntOption.Count))!;
            var option = app.Option("-c|--count", "Count", CommandOptionType.SingleValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse("-c", "42");

            Assert.Equal(42, app.Model.Count);
        }

        [Fact]
        public void AddOption_SingleValue_WithValueTuple_SkipsDefaultValueProcessing()
        {
            var app = new CommandLineApplication<ModelWithValueTuple>();
            var context = new ConventionContext(app, typeof(ModelWithValueTuple));
            var prop = typeof(ModelWithValueTuple).GetProperty(nameof(ModelWithValueTuple.Option))!;
            var option = app.Option("-o|--option", "Option", CommandOptionType.SingleOrNoValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse(); // No value provided

            // Should not throw and should not set default value for value tuple
            Assert.Null(option.DefaultValue);
        }

        #endregion

        #region MultipleValue Option Type Tests

        [Fact]
        public void AddOption_MultipleValue_ParsesValues()
        {
            var app = new CommandLineApplication<ModelWithStringArrayOption>();
            var context = new ConventionContext(app, typeof(ModelWithStringArrayOption));
            var prop = typeof(ModelWithStringArrayOption).GetProperty(nameof(ModelWithStringArrayOption.Values))!;
            var option = app.Option("-v|--values", "Values", CommandOptionType.MultipleValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse("-v", "a", "-v", "b", "-v", "c");

            Assert.NotNull(app.Model.Values);
            Assert.Equal(new[] { "a", "b", "c" }, app.Model.Values);
        }

        [Fact]
        public void AddOption_MultipleValue_UsesDefaultValues()
        {
            var app = new CommandLineApplication<ModelWithStringArrayOptionWithDefault>();
            var context = new ConventionContext(app, typeof(ModelWithStringArrayOptionWithDefault));
            var prop = typeof(ModelWithStringArrayOptionWithDefault).GetProperty(nameof(ModelWithStringArrayOptionWithDefault.Values))!;
            var option = app.Option("-v|--values", "Values", CommandOptionType.MultipleValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse(); // No values provided

            Assert.Equal(new[] { "a", "b" }, app.Model.Values);
            Assert.Equal("a, b", option.DefaultValue);
        }

        [Fact]
        public void AddOption_MultipleValue_NoDefaultWhenNull()
        {
            var app = new CommandLineApplication<ModelWithStringArrayOption>();
            var context = new ConventionContext(app, typeof(ModelWithStringArrayOption));
            var prop = typeof(ModelWithStringArrayOption).GetProperty(nameof(ModelWithStringArrayOption.Values))!;
            var option = app.Option("-v|--values", "Values", CommandOptionType.MultipleValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse(); // No values provided

            Assert.Null(app.Model.Values);
            Assert.Null(option.DefaultValue);
        }

        private class ModelWithObjectArrayOptionWithNulls
        {
            public object?[] Values { get; set; } = new object?[] { "a", null, "b" };
        }

        [Fact]
        public void AddOption_MultipleValue_HandlesNullValuesInDefaults()
        {
            var app = new CommandLineApplication<ModelWithObjectArrayOptionWithNulls>();
            var context = new ConventionContext(app, typeof(ModelWithObjectArrayOptionWithNulls));
            var prop = typeof(ModelWithObjectArrayOptionWithNulls).GetProperty(nameof(ModelWithObjectArrayOptionWithNulls.Values))!;
            var option = app.Option("-v|--values", "Values", CommandOptionType.MultipleValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse(); // No values provided, uses default with nulls

            // Default value should handle nulls gracefully
            Assert.NotNull(option.DefaultValue);
            Assert.Contains("a", option.DefaultValue);
            Assert.Contains("b", option.DefaultValue);
        }

        private class ModelWithEmptyArrayOption
        {
            public string[] Values { get; set; } = Array.Empty<string>();
        }

        [Fact]
        public void AddOption_MultipleValue_NoDefaultWhenEmptyArray()
        {
            var app = new CommandLineApplication<ModelWithEmptyArrayOption>();
            var context = new ConventionContext(app, typeof(ModelWithEmptyArrayOption));
            var prop = typeof(ModelWithEmptyArrayOption).GetProperty(nameof(ModelWithEmptyArrayOption.Values))!;
            var option = app.Option("-v|--values", "Values", CommandOptionType.MultipleValue);

            TestOptionConvention.CallAddOption(context, option, prop);
            app.Parse(); // No values provided

            // Empty array should not set default value
            Assert.Null(option.DefaultValue);
        }

        #endregion

        #region Ambiguous Option Name Tests

        [Fact]
        public void AddOption_ThrowsForAmbiguousShortName_DifferentProperties()
        {
            var app = new CommandLineApplication<ModelWithStringOption>();
            var context = new ConventionContext(app, typeof(ModelWithStringOption));

            // Add first option
            var prop1 = typeof(ModelWithStringOption).GetProperty(nameof(ModelWithStringOption.Value))!;
            var option1 = app.Option("-v|--value1", "Value 1", CommandOptionType.SingleValue);
            TestOptionConvention.CallAddOption(context, option1, prop1);

            // Try to add second option with same short name but different property
            var prop2 = typeof(ModelWithIntOption).GetProperty(nameof(ModelWithIntOption.Count))!;
            var option2 = new CommandOption("-v|--value2", CommandOptionType.SingleValue);
            app.AddOption(option2);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                TestOptionConvention.CallAddOption(context, option2, prop2));

            Assert.Contains("v", ex.Message);
        }

        [Fact]
        public void AddOption_ThrowsForAmbiguousLongName_DifferentProperties()
        {
            var app = new CommandLineApplication<ModelWithStringOption>();
            var context = new ConventionContext(app, typeof(ModelWithStringOption));

            // Add first option with only long name
            var prop1 = typeof(ModelWithStringOption).GetProperty(nameof(ModelWithStringOption.Value))!;
            var option1 = app.Option("--value", "Value 1", CommandOptionType.SingleValue);
            option1.ShortName = ""; // Clear short name
            TestOptionConvention.CallAddOption(context, option1, prop1);

            // Try to add second option with same long name but different property
            var prop2 = typeof(ModelWithIntOption).GetProperty(nameof(ModelWithIntOption.Count))!;
            var option2 = new CommandOption("--value", CommandOptionType.SingleValue);
            option2.ShortName = ""; // Clear short name
            app.AddOption(option2);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                TestOptionConvention.CallAddOption(context, option2, prop2));

            Assert.Contains("value", ex.Message);
        }

        #endregion

        #region Error Cases for Parser Not Found

        private class ModelWithUnsupportedCollectionType
        {
            // A custom type that has no collection parser registered
            public CustomCollection? Items { get; set; }
        }

        private class CustomCollection : System.Collections.Generic.List<object>
        {
        }

        private class ModelWithUnsupportedValueType
        {
            // A custom type that has no value parser registered
            public CustomValueType Value { get; set; }
        }

        private struct CustomValueType
        {
            public int Inner { get; set; }
        }

        [Fact]
        public void AddOption_MultipleValue_ThrowsWhenNoCollectionParser()
        {
            var app = new CommandLineApplication<ModelWithUnsupportedCollectionType>();
            var context = new ConventionContext(app, typeof(ModelWithUnsupportedCollectionType));
            var prop = typeof(ModelWithUnsupportedCollectionType).GetProperty(nameof(ModelWithUnsupportedCollectionType.Items))!;
            var option = app.Option("-i|--items", "Items", CommandOptionType.MultipleValue);

            TestOptionConvention.CallAddOption(context, option, prop);

            // The exception is thrown during parsing, not during setup
            var ex = Assert.Throws<InvalidOperationException>(() => app.Parse("-i", "value"));
            Assert.Contains("Could not automatically determine", ex.Message);
        }

        [Fact]
        public void AddOption_SingleValue_ThrowsWhenNoValueParser()
        {
            var app = new CommandLineApplication<ModelWithUnsupportedValueType>();
            var context = new ConventionContext(app, typeof(ModelWithUnsupportedValueType));
            var prop = typeof(ModelWithUnsupportedValueType).GetProperty(nameof(ModelWithUnsupportedValueType.Value))!;
            var option = app.Option("-v|--value", "Value", CommandOptionType.SingleValue);

            TestOptionConvention.CallAddOption(context, option, prop);

            // The exception is thrown during parsing, not during setup
            var ex = Assert.Throws<InvalidOperationException>(() => app.Parse("-v", "test"));
            Assert.Contains("Could not automatically determine", ex.Message);
        }

        #endregion

        #region EnsureDoesNotHaveArgumentAttribute Tests

        private class ModelWithArgumentAndOption
        {
            [Argument(0)]
            [Option]
            public string? Value { get; set; }
        }

        // Note: EnsureDoesNotHaveArgumentAttribute is tested indirectly through the existing
        // OptionAttributeTests.ThrowsWhenOptionAndArgumentAreSpecified test

        #endregion
    }
}
