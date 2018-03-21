// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Collections.Generic;
using Xunit;
using  McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ValueParserProviderCustomTests
    {

        private class MyDateTimeOffsetParser : IValueParser
        {
            public Type TargetType { get; } = typeof(DateTimeOffset);

            public object Parse(string argName, string value)
            {
                if (!DateTimeOffset.TryParse(value, out var result))
                {
                    throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid date time (with offset)");
                }

                return result;
            }
        }

        // scenario: specialized domain value in the format of 1=123.456=abc
        private class ComplexTupleParser : IValueParser
        {
            public Type TargetType { get; } = typeof(ValueTuple<int, double, string>?);

            public object Parse(string argName, string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return default(ValueTuple<int, double, string>?);
                }

                var fragments = value.Split('=');

                try
                {
                    var item1 = double.Parse(fragments[0]);
                    var item2 = double.Parse(fragments[1]);
                    var item3 = fragments[2];
                    return (ValueTuple<int, double, string>?)(item1, item2, item3);
                }
                catch(Exception ex) 
                {
                    throw new FormatException(
                        $"Invalid value specified for {argName}. '{value} is not a valid time span (with offset)",
                        ex);
                }
            }
        }

        // scenario: for some reason I insist on using thin spaces instead of commas for the thousands delimitters
        private class MyDoubleParser : IValueParser
        {
            // This is a trivial example but rooted in a real standard
            // https://en.wikipedia.org/wiki/ISO_31-0#Numbers
            private readonly NumberFormatInfo _iso80000NumberFormatInfo;

            public MyDoubleParser()
            {
                this._iso80000NumberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

                // a thin space
                this._iso80000NumberFormatInfo.NumberGroupSeparator = "\u2009";
            }

            public Type TargetType { get; } = typeof(double);

            public object Parse(string argName, string value)
            {
                if (!double.TryParse(value, NumberStyles.Number, _iso80000NumberFormatInfo, out var result))
                {
                    throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid ISO80000 double");
                }

                return result;
            }
        }

        private class CustomParserProgram
        {
            [Argument(0)]
            public DateTimeOffset DateTimeOffset { get; }

            [Argument(1)]
            public double Double { get; }

            [Argument(2)]
            public ValueTuple<int, double, string>? ComplexValue { get; }
        }

        [Fact]
        public void CustomParsersCanBeAdded()
        {
            var expectedDate = new DateTimeOffset(2018, 02, 16, 21, 30, 33, 45, TimeSpan.FromHours(10));
            var expectedDouble = 123456.789;
            ValueTuple<int, double, string>? expectedComplexValue = null;

            var app = new CommandLineApplication<CustomParserProgram>();

            app.ValueParsers.AddRange(new IValueParser[] { new MyDateTimeOffsetParser(), new ComplexTupleParser() });
            app.ValueParsers.AddOrReplace(new MyDoubleParser());

            app.Conventions.UseAttributes();

            // We're omitting the third argument to test nullable-ness. The ComplexValue type (with a value) is tested
            // in the next test
            var args = new[] { expectedDate.ToString("O"), "123 456.789" };
            app.Parse(args);
            var model = app.Model;

            Assert.Equal(expectedDate, model.DateTimeOffset);
            Assert.Equal(expectedDouble, model.Double);
            Assert.Equal(expectedComplexValue, model.ComplexValue);
        }

        private class CustomParserProgramOptions
        {
            [Option]
            public ValueTuple<int, double, string>? ComplexValue { get; }
        }

        [Fact]
        public void CustomParsersSupportComplexGenericTypes()
        {
            ValueTuple<int, double, string>? expectedComplexValue = (1, 123.456, "abc");

            var app = new CommandLineApplication<CustomParserProgramOptions>();

            app.ValueParsers.Add(new ComplexTupleParser());

            app.Conventions.UseAttributes();

            var args = $"-c=1=123.456=abc";
            app.Parse(args);
            var model = app.Model;

            Assert.Equal(expectedComplexValue, model.ComplexValue);
        }

        [Fact]
        public void CustomParsersAreAutomaticallySingleValues()
        {
            var app = new CommandLineApplication<CustomParserProgram>();

            app.ValueParsers.AddRange(new IValueParser[] { new MyDateTimeOffsetParser(), new ComplexTupleParser() });
            app.ValueParsers.AddOrReplace(new MyDoubleParser());

            var optionMapper = CommandOptionTypeMapper.Default;
            Assert.Equal(
                CommandOptionType.SingleValue,
                optionMapper.GetOptionType(typeof(DateTimeOffset), app.ValueParsers));
            Assert.Equal(
                CommandOptionType.SingleValue,
                optionMapper.GetOptionType(typeof(ValueTuple<int, double, string>?), app.ValueParsers));
            Assert.Equal(
                CommandOptionType.SingleValue,
                optionMapper.GetOptionType(typeof(double), app.ValueParsers));
        }

        private class BadValueParser : IValueParser
        {
            public Type TargetType { get; } = null;

            public object Parse(string argName, string value)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ThrowsIfNoType()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () =>
                {
                    var app = new CommandLineApplication<CustomParserProgram>();
                    app.ValueParsers.Add(new BadValueParser());
                });

            Assert.Contains("TargetType", ex.Message);
        }

        [Fact]
        public void ThrowsIfAlreadyRegistered()
        {
            var ex = Assert.Throws<ArgumentException>(
                () =>
                {
                    var app = new CommandLineApplication<CustomParserProgram>();
                    app.ValueParsers.Add(new ComplexTupleParser());
                    app.ValueParsers.Add(new ComplexTupleParser());
                });

            Assert.Contains(
                "Value parser provider for type 'System.ValueTuple`3[System.Int32,System.Double,System.String]' already exists.",
                ex.Message);
        }

        [Fact]
        public void AddThrowsIfNullParser()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () =>
                {
                    var app = new CommandLineApplication<CustomParserProgram>();
                    app.ValueParsers.Add(null);
                });

            Assert.Contains("parser", ex.Message);
        }

        [Fact]
        public void AddRangeThrowsIfNullCollection()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () =>
                {
                    var app = new CommandLineApplication<CustomParserProgram>();
                    app.ValueParsers.AddRange(null);
                });

            Assert.Contains("parsers", ex.Message);
        }

        [Fact]
        public void AddOrReplaceThrowsIfNullparser()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () =>
                {
                    var app = new CommandLineApplication<CustomParserProgram>();
                    app.ValueParsers.AddOrReplace(null);
                });

            Assert.Contains("parser", ex.Message);
        }
    }
}
