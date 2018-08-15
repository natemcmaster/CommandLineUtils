// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils.Abstractions;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ValueParserProviderCustomTests
    {

        internal class MyDateTimeOffsetParser : IValueParser
        {
            public Type TargetType { get; } = typeof(DateTimeOffset);

            public object Parse(string argName, string value, CultureInfo culture)
            {
                if (!DateTimeOffset.TryParse(value, culture, DateTimeStyles.None, out var result))
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

            public object Parse(string argName, string value, CultureInfo culture)
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

            public object Parse(string argName, string value, CultureInfo culture)
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


        private class DateParserProgram
        {
            [Argument(0)]
            public DateTimeOffset DateTimeOffset { get; }
        }

        [Theory]
        [InlineData(nameof(DateParserProgram.DateTimeOffset), "03/30/2017 3:03:03 +04:00", "en-US")]
        [InlineData(nameof(DateParserProgram.DateTimeOffset), "2017年03月30日 3:03:03 +04:00", "zh-CN")]
        public void DefaultCultureCanBeChanged(string property, string test, string culture)
        {
            var expected = new DateTimeOffset(2017, 3, 30, 3, 3, 3, TimeSpan.FromHours(4));

            var cultureInfo = new CultureInfo(culture);
            var app = new CommandLineApplication<DateParserProgram>();
            app.ValueParsers.ParseCulture = cultureInfo;
            app.ValueParsers.Add(new MyDateTimeOffsetParser());
            app.Conventions.UseAttributes();
            app.Parse(test);

            var actual = (DateTimeOffset)typeof(DateParserProgram).GetProperty(property).GetMethod.Invoke(app.Model, null);
            Assert.Equal(expected, actual);
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


        [Command()]
        [Subcommand(typeof(CustomParserProgramAttributesSubCommand))]
        private class CustomParserProgramAttributes
        {
            [Option("-a")]
            public DateTimeOffset MainDate { get; }
        }

        [Command("subcommand")]
        private class CustomParserProgramAttributesSubCommand
        {
            [Option("-b")]
            public DateTimeOffset SubDate { get; }

            public Task<int> OnExecute(CommandLineApplication app)
            {
                return Task.FromResult(1);
            }
        }

        [Fact]
        public void CustomParsersAreAvailableToSubCommands()
        {
            var expectedDate = new DateTimeOffset(2018, 02, 16, 21, 30, 33, 45, TimeSpan.FromHours(10));


            var app = new CommandLineApplication<CustomParserProgramAttributes>();
            app.ValueParsers.AddOrReplace(new MyDateTimeOffsetParser());
            app.Conventions.UseDefaultConventions();

            var args = new[] { "-a", expectedDate.ToString("O"), "subcommand", "-b", expectedDate.AddSeconds(123456).ToString("O") };

            var result = app.Execute(args);

            Assert.Equal(1, result);
            Assert.Equal(expectedDate, app.Model.MainDate);
            Assert.Equal(expectedDate.AddSeconds(123456), ((CommandLineApplication<CustomParserProgramAttributesSubCommand>)app.Commands[0]).Model.SubDate);
        }

        [Fact]
        public void CustomParsersAreAvailableToBuilderSubCommands()
        {
            var expectedDate = new DateTimeOffset(2018, 02, 16, 21, 30, 33, 45, TimeSpan.FromHours(10));
            DateTimeOffset actualMainDate = default;
            DateTimeOffset actualSubDate = default;

            var app = new CommandLineApplication();
            app.ValueParsers.AddOrReplace(new MyDateTimeOffsetParser());

            var mainDate = app.Option<DateTimeOffset>("-a", "The main date to parse", CommandOptionType.SingleValue);
            app.Command("subcommand", configCmd =>
            {
                var subDate = configCmd.Option<DateTimeOffset>("-b", "A date for the sub command", CommandOptionType.SingleValue);

                configCmd.OnExecute(() =>
                {
                    actualMainDate = mainDate.ParsedValue;
                    actualSubDate = subDate.ParsedValue;
                });
            });

            var args = new[] { "-a", expectedDate.ToString("O"), "subcommand", "-b", expectedDate.AddSeconds(123456).ToString("O") };
            app.Execute(args);

            Assert.Equal(expectedDate, actualMainDate);
            Assert.Equal(expectedDate.AddSeconds(123456), actualSubDate);
        }

        private class BadValueParser : IValueParser
        {
            public Type TargetType { get; } = null;

            public object Parse(string argName, string value, CultureInfo culture)
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
