// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class OptionMetadataTests
    {
        private class TestModel
        {
            public string? Name { get; set; }
            public int Count { get; set; }
            public bool Verbose { get; set; }
            public string[]? Values { get; set; }
        }

        [Fact]
        public void Constructor_SetsRequiredProperties()
        {
            var metadata = new OptionMetadata(
                propertyName: "Name",
                propertyType: typeof(string),
                getter: obj => ((TestModel)obj).Name,
                setter: (obj, val) => ((TestModel)obj).Name = (string?)val);

            Assert.Equal("Name", metadata.PropertyName);
            Assert.Equal(typeof(string), metadata.PropertyType);
        }

        [Fact]
        public void GetterAndSetter_WorkCorrectly()
        {
            var metadata = new OptionMetadata(
                propertyName: "Name",
                propertyType: typeof(string),
                getter: obj => ((TestModel)obj).Name,
                setter: (obj, val) => ((TestModel)obj).Name = (string?)val);

            var model = new TestModel();

            metadata.Setter(model, "test-value");
            Assert.Equal("test-value", model.Name);

            var retrieved = metadata.Getter(model);
            Assert.Equal("test-value", retrieved);
        }

        [Fact]
        public void OptionalProperties_HaveDefaults()
        {
            var metadata = new OptionMetadata(
                propertyName: "Name",
                propertyType: typeof(string),
                getter: obj => ((TestModel)obj).Name,
                setter: (obj, val) => ((TestModel)obj).Name = (string?)val);

            Assert.Null(metadata.Template);
            Assert.Null(metadata.ShortName);
            Assert.Null(metadata.LongName);
            Assert.Null(metadata.SymbolName);
            Assert.Null(metadata.ValueName);
            Assert.Null(metadata.Description);
            // Default is MultipleValue (enum value 0)
            Assert.Equal(CommandOptionType.MultipleValue, metadata.OptionType);
            Assert.True(metadata.ShowInHelpText);
            Assert.False(metadata.Inherited);
            Assert.NotNull(metadata.Validators);
            Assert.Empty(metadata.Validators);
        }

        [Fact]
        public void OptionalProperties_CanBeSet()
        {
            var validators = new List<ValidationAttribute> { new RequiredAttribute() };

            var metadata = new OptionMetadata(
                propertyName: "Count",
                propertyType: typeof(int),
                getter: obj => ((TestModel)obj).Count,
                setter: (obj, val) => ((TestModel)obj).Count = (int)val!)
            {
                Template = "-c|--count",
                ShortName = "c",
                LongName = "count",
                SymbolName = "COUNT",
                ValueName = "NUMBER",
                Description = "The count value",
                OptionType = CommandOptionType.SingleValue,
                ShowInHelpText = false,
                Inherited = true,
                Validators = validators
            };

            Assert.Equal("-c|--count", metadata.Template);
            Assert.Equal("c", metadata.ShortName);
            Assert.Equal("count", metadata.LongName);
            Assert.Equal("COUNT", metadata.SymbolName);
            Assert.Equal("NUMBER", metadata.ValueName);
            Assert.Equal("The count value", metadata.Description);
            Assert.Equal(CommandOptionType.SingleValue, metadata.OptionType);
            Assert.False(metadata.ShowInHelpText);
            Assert.True(metadata.Inherited);
            Assert.Same(validators, metadata.Validators);
        }

        [Fact]
        public void WorksWithValueTypes()
        {
            var metadata = new OptionMetadata(
                propertyName: "Count",
                propertyType: typeof(int),
                getter: obj => ((TestModel)obj).Count,
                setter: (obj, val) => ((TestModel)obj).Count = (int)val!);

            var model = new TestModel();

            metadata.Setter(model, 42);
            Assert.Equal(42, model.Count);

            var retrieved = metadata.Getter(model);
            Assert.Equal(42, retrieved);
        }

        [Fact]
        public void WorksWithBooleanType()
        {
            var metadata = new OptionMetadata(
                propertyName: "Verbose",
                propertyType: typeof(bool),
                getter: obj => ((TestModel)obj).Verbose,
                setter: (obj, val) => ((TestModel)obj).Verbose = (bool)val!);

            var model = new TestModel();

            metadata.Setter(model, true);
            Assert.True(model.Verbose);

            var retrieved = metadata.Getter(model);
            Assert.Equal(true, retrieved);
        }

        [Fact]
        public void WorksWithArrayType()
        {
            var metadata = new OptionMetadata(
                propertyName: "Values",
                propertyType: typeof(string[]),
                getter: obj => ((TestModel)obj).Values,
                setter: (obj, val) => ((TestModel)obj).Values = (string[]?)val)
            {
                OptionType = CommandOptionType.MultipleValue
            };

            var model = new TestModel();
            var values = new[] { "a", "b", "c" };

            metadata.Setter(model, values);
            Assert.Same(values, model.Values);

            var retrieved = metadata.Getter(model);
            Assert.Same(values, retrieved);
        }
    }
}
