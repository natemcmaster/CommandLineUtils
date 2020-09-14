// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    using System.Reflection;

    public class ValueParserProviderTests
    {
        public enum Color
        {
            Red,
            Blue,
            Green,
        }

        private class Program
        {
            [Option("--byte")]
            public byte Byte { get; }

            [Option("--int16")]
            public short Int16 { get; }

            [Option("--int32")]
            public int Int32 { get; }

            [Option("--int64")]
            public long Int64 { get; }

            [Option("--float")]
            public float Float { get; }

            [Option("--double")]
            public double Double { get; }

            [Option("--bool", CommandOptionType.SingleValue)]
            public bool Bool { get; }

            [Option("--bool-opt", CommandOptionType.SingleValue)]
            public bool? BoolOpt { get; }

            [Option("--int32-opt")]
            public int? Int32Opt { get; }

            [Option("--int64-opt")]
            public long? Int64Opt { get; }

            [Option("--float-opt")]
            public float? FloatOpt { get; }

            [Option("--double-opt")]
            public double? DoubleOpt { get; }

            [Option("--uint32")]
            public uint UInt32 { get; }

            [Option("--uint64")]
            public ulong UInt64 { get; }

            [Option("--string-arr")]
            public string[]? StringArray { get; }

            [Option("--int32-arr")]
            public int[]? Int32Array { get; }

            [Option("--flag")]
            public bool[]? Flags { get; }

            [Option("--string-ilist")]
            public IList<string>? StringIList { get; }

            [Option("--string-list")]
            public List<string>? StringList { get; }

            [Option("--string-set")]
            public ISet<string>? StringSet { get; }

            [Option("--color")]
            public Color Color { get; }

            [Option("--color-opt")]
            public Color? ColorOpt { get; }

            [Option("--value-tuple:<VALUE>")]
            public (bool HasValue, string Value) ValueTuple { get; }

            [Option("--int-tuple:<VALUE>")]
            public (bool HasValue, int count) IntTuple { get; }

            [Option("--int-tuple-opt:<VALUE>")]
            public (bool HasValue, int? count) IntOptTuple { get; }

            [Option("--color-value-tuple:<VALUE>")]
            public (bool HasValue, Color Value) EnumValueTuple { get; }

            [Option("--uri")]
            public Uri? Uri { get; set; }

            [Option("--datetime")]
            public DateTime DateTime { get; }

            [Option("--datetime-offset")]
            public DateTimeOffset DateTimeOffset { get; }

            [Option("--timespan")]
            public TimeSpan TimeSpan { get; }

            [Option("--guid", CommandOptionType.SingleValue)]
            public Guid Guid { get; }

            [Option("--guid-opt", CommandOptionType.SingleValue)]
            public Guid? GuidOpt { get; }
        }

        private sealed class InCulture : IDisposable
        {
            private readonly CultureInfo _oldCultureInfo;

            public InCulture(CultureInfo newCultureInfo)
            {
                _oldCultureInfo = CultureInfo.CurrentCulture;
                CultureInfo.CurrentCulture = newCultureInfo;
            }

            public void Dispose()
            {
                CultureInfo.CurrentCulture = _oldCultureInfo;
            }
        }

        // Workaround https://github.com/dotnet/roslyn/issues/33199 https://github.com/xunit/xunit/issues/1897
#nullable disable
        public static IEnumerable<object[]> GetFloatingPointSymbolsData()
        {
            using (new InCulture(CultureInfo.InvariantCulture))
            {
                var format = CultureInfo.CurrentCulture.NumberFormat;
                return new[]
                {
                    new object[] { format.PositiveInfinitySymbol, float.PositiveInfinity },
                    new object[] { format.NegativeInfinitySymbol, float.NegativeInfinity },
                    new object[] { format.NaNSymbol, float.NaN},
                };
            }
        }
#nullable enable

        [Theory]
        [InlineData("255", byte.MaxValue)]
        [InlineData("1", 1)]
        [InlineData("0", 0)]
        public void ParsesByte(string arg, byte result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--byte", arg);
            Assert.Equal(result, parsed.Byte);
        }

        [Theory]
        [InlineData("32767", short.MaxValue)]
        [InlineData("1", 1)]
        [InlineData("0", 0)]
        [InlineData("-1", -1)]
        [InlineData("-32768", short.MinValue)]
        public void ParsesShort(string arg, short result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--int16", arg);
            Assert.Equal(result, parsed.Int16);
        }

        [Theory]
        [InlineData("2147483647", int.MaxValue)]
        [InlineData("1", 1)]
        [InlineData("0", 0)]
        [InlineData("-1", -1)]
        [InlineData("-2147483648", int.MinValue)]
        public void ParsesInt(string arg, int result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--int32", arg);
            Assert.Equal(result, parsed.Int32);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("", null)]
        public void ParsesIntNullable(string arg, int? result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--int32-opt", arg);
            Assert.Equal(result, parsed.Int32Opt);
        }

        [Theory]
        [InlineData("9223372036854775807", long.MaxValue)]
        [InlineData("1", 1)]
        [InlineData("0", 0)]
        [InlineData("-1", -1)]
        [InlineData("-9223372036854775808", long.MinValue)]
        public void ParsesLong(string arg, long result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--int64", arg);
            Assert.Equal(result, parsed.Int64);
        }

        [Theory]
        [InlineData("9223372036854775807", long.MaxValue)]
        [InlineData("", null)]
        public void ParsesLongNullable(string arg, long? result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--int64-opt", arg);
            Assert.Equal(result, parsed.Int64Opt);
        }

        [Theory]
        [InlineData("0.0", 0.0f)]
        [InlineData("123456789.987654321", 123456789.987654321f)]
        [InlineData("-123.456", -123.456f)]
        [InlineData("-1E10", -1E10f)]
        [MemberData(nameof(GetFloatingPointSymbolsData))]
        public void ParsesFloat(string arg, float result)
        {
            using (new InCulture(CultureInfo.InvariantCulture))
            {
                var parsed = CommandLineParser.ParseArgs<Program>("--float", arg);
                Assert.Equal(result, parsed.Float);
            }
        }

        [Theory]
        [InlineData("0.0", 0.0)]
        [InlineData("123456789.987654321", 123456789.987654321)]
        [InlineData("-123.456", -123.456)]
        [InlineData("-1E10", -1E10)]
        [MemberData(nameof(GetFloatingPointSymbolsData))]
        public void ParsesDouble(string arg, double result)
        {
            using (new InCulture(CultureInfo.InvariantCulture))
            {
                var parsed = CommandLineParser.ParseArgs<Program>("--double", arg);
                Assert.Equal(result, parsed.Double);
            }
        }

        [Theory]
        [InlineData("0.0", 0.0f)]
        [InlineData("123456789.987654321", 123456789.987654321f)]
        [InlineData("-123.456", -123.456f)]
        [InlineData("-1E10", -1E10f)]
        [MemberData(nameof(GetFloatingPointSymbolsData))]
        [InlineData("", null)]
        public void ParsesFloatNullable(string arg, float? result)
        {
            using (new InCulture(CultureInfo.InvariantCulture))
            {
                var parsed = CommandLineParser.ParseArgs<Program>("--float-opt", arg);
                Assert.Equal(result, parsed.FloatOpt);
            }
        }

        [Theory]
        [InlineData("0.0", 0.0)]
        [InlineData("123456789.987654321", 123456789.987654321)]
        [InlineData("-123.456", -123.456)]
        [InlineData("-1E10", -1E10)]
        [MemberData(nameof(GetFloatingPointSymbolsData))]
        [InlineData("", null)]
        public void ParsesDoubleNullable(string arg, double? result)
        {
            using (new InCulture(CultureInfo.InvariantCulture))
            {
                var parsed = CommandLineParser.ParseArgs<Program>("--double-opt", arg);
                Assert.Equal(result, parsed.DoubleOpt);
            }
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("False", false)]
        [InlineData("false", false)]
        public void ParsesBool(string arg, bool result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--bool", arg);
            Assert.Equal(result, parsed.Bool);
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("False", false)]
        [InlineData("false", false)]
        public void ParsesNullableBool(string arg, bool? result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--bool-opt", arg);
            Assert.Equal(result, parsed.BoolOpt);
        }

        [Theory]
        [InlineData("4294967295", uint.MaxValue)]
        [InlineData("1", 1)]
        [InlineData("0", 0)]
        public void ParseUInt(string arg, uint result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--uint32", arg);
            Assert.Equal(result, parsed.UInt32);
        }

        [Theory]
        [InlineData("18446744073709551615", ulong.MaxValue)]
        [InlineData("1", 1)]
        [InlineData("0", 0)]
        public void ParsesULong(string arg, ulong result)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--uint64", arg);
            Assert.Equal(result, parsed.UInt64);
        }

        [Fact]
        public void ParsesInt32Array()
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--int32-arr", "-1", "--int32-arr", "1");
            Assert.Equal(-1, parsed.Int32Array?[0]);
            Assert.Equal(1, parsed.Int32Array?[1]);
        }

        [Fact]
        public void ParsesStringArray()
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--string-arr", "first", "--string-arr", "second");
            Assert.Equal("first", parsed.StringArray?[0]);
            Assert.Equal("second", parsed.StringArray?[1]);
        }

        [Fact]
        public void ParsesStringIList()
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--string-ilist", "first", "--string-ilist", "second");
            Assert.Equal("first", parsed.StringIList?[0]);
            Assert.Equal("second", parsed.StringIList?[1]);
        }

        [Fact]
        public void ParsesStringList()
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--string-list", "first", "--string-list", "second");
            Assert.Equal("first", parsed.StringList?[0]);
            Assert.Equal("second", parsed.StringList?[1]);
        }


        [Fact]
        public void ParsesStringSet()
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--string-set", "first", "--string-set", "second");
            Assert.Contains("first", parsed.StringSet);
            Assert.Contains("second", parsed.StringSet);
        }

        [Theory]
        [InlineData(Color.Blue)]
        [InlineData(Color.Red)]
        [InlineData(Color.Green)]
        public void ParsesEnum(Color color)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--color", color.ToString().ToLowerInvariant());
            Assert.Equal(color, parsed.Color);
        }

        [Theory]
        [InlineData("--value-tuple", null)]
        [InlineData("--value-tuple:", "")]
        [InlineData("--value-tuple: ", " ")]
        [InlineData("--value-tuple:path", "path")]
        public void ParsesValueTupleOfBoolAndType(string input, string expected)
        {
            var parsed = CommandLineParser.ParseArgs<Program>(input);
            Assert.True(parsed.ValueTuple.HasValue);
            Assert.Equal(expected, parsed.ValueTuple.Value);
        }

        [Theory]
        [InlineData("--int-tuple", 0)]
        [InlineData("--int-tuple:1", 1)]
        public void ParsesTupleOfBoolAndInt(string input, int expected)
        {
            var parsed = CommandLineParser.ParseArgs<Program>(input);
            Assert.True(parsed.IntTuple.HasValue);
            Assert.Equal(expected, parsed.IntTuple.count);
        }

        [Theory]
        [InlineData("--int-tuple-opt", null)]
        [InlineData("--int-tuple-opt:1", 1)]
        public void ParsesTupleOfBoolAndNullableInt(string input, int? expected)
        {
            var parsed = CommandLineParser.ParseArgs<Program>(input);
            Assert.True(parsed.IntOptTuple.HasValue);
            Assert.Equal(expected, parsed.IntOptTuple.count);
        }

        [Theory]
        [InlineData("--color-value-tuple", default(Color))]
        [InlineData("--color-value-tuple:Red", Color.Red)]
        [InlineData("--color-value-tuple:green", Color.Green)]
        [InlineData("--color-value-tuple:BLUE", Color.Blue)]
        public void ParsesValueTupleOfBoolAndEnum(string input, Color expected)
        {
            var parsed = CommandLineParser.ParseArgs<Program>(input);
            Assert.True(parsed.EnumValueTuple.HasValue);
            Assert.Equal(expected, parsed.EnumValueTuple.Value);
        }

        [Theory]
        [InlineData(Color.Blue)]
        [InlineData(Color.Red)]
        [InlineData(Color.Green)]
        public void ParsesNullableEnum(Color? color)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--color-opt", color!.ToString()!.ToLowerInvariant());
            Assert.True(parsed.ColorOpt.HasValue, "Option should have value");
            Assert.Equal(color, parsed.ColorOpt);
        }

        [Theory]
        [InlineData("http://example.com/")]
        [InlineData("http://example.com/foo/bar")]
        [InlineData("/foo/bar")]
        public void ParsesUri(string uriString)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--uri", uriString);
            Assert.Equal(uriString, parsed.Uri?.ToString());
        }

        [Theory]
        [InlineData("2009-06-15")]
        [InlineData("2009-06-15T13:45:30")]
        [InlineData("2009-06-15T13:45:30Z")]
        public void ParseDateTime(string dateTimeString)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--datetime", dateTimeString);
            var expected = DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            Assert.Equal(expected, parsed.DateTime);
            Assert.Equal(expected.Kind, parsed.DateTime.Kind);
        }

        [Theory]
        [InlineData("2009-06-15")]
        [InlineData("2009-06-15T13:45:30Z")]
        [InlineData("2009-06-15T13:45:30+08:00")]
        public void ParseDateTimeOffset(string dateTimeOffsetString)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--datetime-offset", dateTimeOffsetString);
            Assert.Equal(DateTimeOffset.Parse(dateTimeOffsetString), parsed.DateTimeOffset);
        }

        [Theory]
        [InlineData("00:30:00")]
        [InlineData("-10:27:22")]
        [InlineData("1:1:57:54")]
        public void ParseTimeSpan(string timeSpanString)
        {
            var parsed = CommandLineParser.ParseArgs<Program>("--timespan", timeSpanString);
            Assert.Equal(TimeSpan.Parse(timeSpanString), parsed.TimeSpan);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public void ParsesBoolArray(int repeat)
        {
            var args = new List<string>();
            for (var i = 0; i < repeat; i++)
            {
                args.Add("--flag");
            }

            var parsed = CommandLineParser.ParseArgs<Program>(args.ToArray());
            Assert.NotNull(parsed.Flags);
            Assert.Equal(repeat, parsed.Flags?.Length);
            Assert.All(parsed.Flags, value => Assert.True(value));
        }

        [Theory]
        [InlineData("ff23ef12-500a-48df-9a5d-151c2adc2a0a")]
        [InlineData("ff23ef12500a48df9a5d151c2adc2a0a")]
        [InlineData("{ff23ef12-500a-48df-9a5d-151c2adc2a0a}")]
        [InlineData("(ff23ef12-500a-48df-9a5d-151c2adc2a0a)")]
        [InlineData("{0xff23ef12,0x500a,0x48df,{0x9a,0x5d,0x15,0x1c,0x2a,0xdc,0x2a,0x0a}}")]
        public void ParsesGuid(string arg)
        {
            var expected = Guid.Parse("ff23ef12-500a-48df-9a5d-151c2adc2a0a");
            var parsed = CommandLineParser.ParseArgs<Program>("--guid", arg);
            Assert.Equal(expected, parsed.Guid);
        }

        [Theory]
        [InlineData("ff23ef12-500a-48df-9a5d-151c2adc2a0a")]
        [InlineData("ff23ef12500a48df9a5d151c2adc2a0a")]
        [InlineData("{ff23ef12-500a-48df-9a5d-151c2adc2a0a}")]
        [InlineData("(ff23ef12-500a-48df-9a5d-151c2adc2a0a)")]
        [InlineData("{0xff23ef12,0x500a,0x48df,{0x9a,0x5d,0x15,0x1c,0x2a,0xdc,0x2a,0x0a}}")]
        [InlineData("")]
        public void ParsesGuidNullable(string arg)
        {
            var expected = string.IsNullOrWhiteSpace(arg)
                ? (Guid?)null
                : Guid.Parse("ff23ef12-500a-48df-9a5d-151c2adc2a0a");
            var parsed = CommandLineParser.ParseArgs<Program>("--guid-opt", arg);
            Assert.Equal(expected, parsed.GuidOpt);
        }

        [Theory]
        [InlineData(nameof(Program.Float), "--float", "123.456,7", "de-DE", 123456.7f)]
        [InlineData(nameof(Program.Double), "--double", "123.456,789", "de-DE", 123456.789)]
        public void DefaultCultureCanBeChanged(string property, string option, string test, string culture, object expected)
        {
            var cultureInfo = new CultureInfo(culture);
            var app = new CommandLineApplication<Program>();
            app.ValueParsers.ParseCulture = cultureInfo;
            app.Conventions.UseAttributes();
            app.Parse(option, test);

            var actual = typeof(Program).GetProperty(property)!.GetMethod!.Invoke(app.Model, null);
            Assert.Equal(expected, actual);
        }

        private class ArgumentProgram
        {
            [Argument(0)]
            public int Int32Arg { get; }

            [Argument(1)]
            public bool BoolArg { get; }

            [Argument(2)]
            public IList<string>? TheRest { get; }
        }

        [Fact]
        public void ParsesArgs()
        {
            var parsed = CommandLineParser.ParseArgs<ArgumentProgram>(
                "1",
                "true",
                "a",
                "b");

            Assert.Equal(1, parsed.Int32Arg);
            Assert.True(parsed.BoolArg);
            Assert.Equal(2, parsed.TheRest?.Count);
            Assert.Equal("a", parsed.TheRest?[0]);
            Assert.Equal("b", parsed.TheRest?[1]);
        }

        [Fact]
        public void ThrowsIfNoValueParserProviderIsAvailable()
        {
            var app = new CommandLineApplication();
            var ex = Assert.Throws<InvalidOperationException>(() => app.Option<ValueParserProviderTests>("-t", "test", CommandOptionType.NoValue));
            Assert.Equal(Strings.CannotDetermineParserType(typeof(ValueParserProviderTests)), ex.Message);
            ex = Assert.Throws<InvalidOperationException>(() => app.Argument<ValueParserProviderTests>("-t", "test"));
            Assert.Equal(Strings.CannotDetermineParserType(typeof(ValueParserProviderTests)), ex.Message);
        }

        [Theory]
        [InlineData(typeof(bool), "0", false)]
        [InlineData(typeof(bool), "1", true)]
        [InlineData(typeof(bool), "T", true)]
        [InlineData(typeof(bool), "true", true)]
        [InlineData(typeof(bool), "false", false)]
        [InlineData(typeof(bool), "F", false)]
        [InlineData(typeof(long), "9223372036854775807", long.MaxValue)]
        [InlineData(typeof(int), "2147483647", int.MaxValue)]
        [InlineData(typeof(short), "128", (short)128)]
        [InlineData(typeof(byte), "4", (byte)4)]
        [InlineData(typeof(ulong), "56", 56ul)]
        [InlineData(typeof(double), "3.14", 3.14)]
        [InlineData(typeof(float), "3.14", 3.14f)]
        [InlineData(typeof(int?), "", null)]
        [InlineData(typeof(Color), "blue", Color.Blue)]
        [InlineData(typeof(Color), null, default(Color))]
        [InlineData(typeof(Color?), "", null)]
        [MemberData(nameof(TupleData))]
        public void ItParsesParamofT(Type type, string input, object expected)
        {
            using (new InCulture(CultureInfo.InvariantCulture))
            {
                s_ItParsesOptionOfTImpl.MakeGenericMethod(type).Invoke(this, new[] { input, expected });
                s_ItParsesArgumentOfTImpl.MakeGenericMethod(type).Invoke(this, new[] { input, expected });
            }
        }

        public static TheoryData<Type, string, object> TupleData
            => new TheoryData<Type, string, object>
                {
                    {typeof((bool, int?)), "", (true, default(int?))},
                    {typeof((bool, int)), "123", (true, 123)},
                };

        private static readonly MethodInfo s_ItParsesOptionOfTImpl
                  = typeof(ValueParserProviderTests).GetMethod(nameof(ItParsesOptionOfTImpl), BindingFlags.NonPublic | BindingFlags.Instance)!;

        private void ItParsesOptionOfTImpl<T>(string input, T expected)
        {
            var app = new CommandLineApplication();
            var option = app.Option<T>("--value", "", CommandOptionType.SingleValue);
            app.Parse("--value", input);
            Assert.True(option.HasValue());
            Assert.Equal(expected, option.ParsedValue);
        }

        private static readonly MethodInfo s_ItParsesArgumentOfTImpl
                  = typeof(ValueParserProviderTests).GetMethod(nameof(ItParsesArgumentOfTImpl), BindingFlags.NonPublic | BindingFlags.Instance)!;

        private void ItParsesArgumentOfTImpl<T>(string input, T expected)
        {
            var app = new CommandLineApplication();
            var argument = app.Argument<T>("value", "");
            app.Parse(input);
            Assert.Equal(expected, argument.ParsedValue);
        }
    }
}
