// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests.SourceGeneration
{
    public class ArgumentMetadataTests
    {
        private class TestModel
        {
            public string? File { get; set; }
            public string[]? Files { get; set; }
            public int Priority { get; set; }
        }

        [Fact]
        public void Constructor_SetsRequiredProperties()
        {
            var metadata = new ArgumentMetadata(
                propertyName: "File",
                propertyType: typeof(string),
                order: 0,
                getter: obj => ((TestModel)obj).File,
                setter: (obj, val) => ((TestModel)obj).File = (string?)val);

            Assert.Equal("File", metadata.PropertyName);
            Assert.Equal(typeof(string), metadata.PropertyType);
            Assert.Equal(0, metadata.Order);
        }

        [Fact]
        public void GetterAndSetter_WorkCorrectly()
        {
            var metadata = new ArgumentMetadata(
                propertyName: "File",
                propertyType: typeof(string),
                order: 0,
                getter: obj => ((TestModel)obj).File,
                setter: (obj, val) => ((TestModel)obj).File = (string?)val);

            var model = new TestModel();

            metadata.Setter(model, "test-file.txt");
            Assert.Equal("test-file.txt", model.File);

            var retrieved = metadata.Getter(model);
            Assert.Equal("test-file.txt", retrieved);
        }

        [Fact]
        public void OptionalProperties_HaveDefaults()
        {
            var metadata = new ArgumentMetadata(
                propertyName: "File",
                propertyType: typeof(string),
                order: 0,
                getter: obj => ((TestModel)obj).File,
                setter: (obj, val) => ((TestModel)obj).File = (string?)val);

            Assert.Null(metadata.Name);
            Assert.Null(metadata.Description);
            Assert.True(metadata.ShowInHelpText);
            Assert.False(metadata.MultipleValues);
            Assert.NotNull(metadata.Validators);
            Assert.Empty(metadata.Validators);
        }

        [Fact]
        public void OptionalProperties_CanBeSet()
        {
            var validators = new List<ValidationAttribute> { new RequiredAttribute() };

            var metadata = new ArgumentMetadata(
                propertyName: "File",
                propertyType: typeof(string),
                order: 1,
                getter: obj => ((TestModel)obj).File,
                setter: (obj, val) => ((TestModel)obj).File = (string?)val)
            {
                Name = "file",
                Description = "The file to process",
                ShowInHelpText = false,
                MultipleValues = false,
                Validators = validators
            };

            Assert.Equal("file", metadata.Name);
            Assert.Equal("The file to process", metadata.Description);
            Assert.False(metadata.ShowInHelpText);
            Assert.False(metadata.MultipleValues);
            Assert.Same(validators, metadata.Validators);
        }

        [Fact]
        public void Order_DeterminesSortOrder()
        {
            var first = new ArgumentMetadata(
                propertyName: "First",
                propertyType: typeof(string),
                order: 0,
                getter: obj => null,
                setter: (obj, val) => { });

            var second = new ArgumentMetadata(
                propertyName: "Second",
                propertyType: typeof(string),
                order: 1,
                getter: obj => null,
                setter: (obj, val) => { });

            var third = new ArgumentMetadata(
                propertyName: "Third",
                propertyType: typeof(string),
                order: 2,
                getter: obj => null,
                setter: (obj, val) => { });

            var arguments = new List<ArgumentMetadata> { third, first, second };
            arguments.Sort((a, b) => a.Order.CompareTo(b.Order));

            Assert.Equal("First", arguments[0].PropertyName);
            Assert.Equal("Second", arguments[1].PropertyName);
            Assert.Equal("Third", arguments[2].PropertyName);
        }

        [Fact]
        public void WorksWithArrayType_MultipleValues()
        {
            var metadata = new ArgumentMetadata(
                propertyName: "Files",
                propertyType: typeof(string[]),
                order: 0,
                getter: obj => ((TestModel)obj).Files,
                setter: (obj, val) => ((TestModel)obj).Files = (string[]?)val)
            {
                MultipleValues = true
            };

            var model = new TestModel();
            var values = new[] { "file1.txt", "file2.txt", "file3.txt" };

            metadata.Setter(model, values);
            Assert.Same(values, model.Files);
            Assert.True(metadata.MultipleValues);
        }

        [Fact]
        public void WorksWithValueType()
        {
            var metadata = new ArgumentMetadata(
                propertyName: "Priority",
                propertyType: typeof(int),
                order: 0,
                getter: obj => ((TestModel)obj).Priority,
                setter: (obj, val) => ((TestModel)obj).Priority = (int)val!);

            var model = new TestModel();

            metadata.Setter(model, 5);
            Assert.Equal(5, model.Priority);

            var retrieved = metadata.Getter(model);
            Assert.Equal(5, retrieved);
        }
    }
}
