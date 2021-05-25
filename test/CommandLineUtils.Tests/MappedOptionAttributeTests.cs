// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using FluentAssertions.Execution;
using McMaster.Extensions.CommandLineUtils.Attributes;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class MappedOptionAttributeTests : ConventionTestBase
    {
        public MappedOptionAttributeTests(ITestOutputHelper output) : base(output)
        {
        }

        protected CommandLineApplication<T> Create<T>() where T : class
            => Create<T, MappedOptionAttributeConvention>();

        private class AppWithUnknownOptionType
        {
            [MappedOption("--blubb", "blubb")] public MappedOptionAttributeTests? Option { get; }
        }

        [Fact]
        public void ThrowsWhenOptionTypeCannotBeDetermined()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Create<AppWithUnknownOptionType>());
            Assert.Equal(
                Strings.CannotDetermineOptionType(typeof(AppWithUnknownOptionType).GetProperty("Option")),
                ex.Message);
        }

        private class ShortNameOverride
        {
            [MappedOption(nameof(Detail1), ShortName = "d1")]
            public string? Detail1 { get; set; }

            [MappedOption(nameof(Detail2), ShortName = "d2")]
            public string? Detail2 { get; set; }
        }

        [Fact]
        public void CanOverrideShortNameOnOption()
        {
            var app = Create<ShortNameOverride>();

            using var assertionScope = new AssertionScope();

            var d1 = app.GetOptions()
                .Should().ContainSingle(o => o.ShortName == "d1").Subject;
            d1.ShortName.Should().Be("d1");
            d1.LongName.Should().Be("detail1");
            d1.ValueName.Should().BeNull();

            var d2 = app.GetOptions()
                .Should().ContainSingle(o => o.ShortName == "d2").Subject;
            d2.ShortName.Should().Be("d2");
            d2.LongName.Should().Be("detail2");
            d2.ValueName.Should().BeNull();
        }

        private class EmptyShortName
        {
            [MappedOption(nameof(Detail1), ShortName = "")]
            public string? Detail1 { get; set; }

            [MappedOption(nameof(Detail2), ShortName = "")]
            public string? Detail2 { get; set; }
        }

        [Fact]
        public void CanSetShortNameToEmptyString()
        {
            var app = Create<EmptyShortName>();

            app.GetOptions().Should().HaveCount(2)
                .And.OnlyContain(o => string.IsNullOrEmpty(o.ShortName));
        }

        /*
        private class AmbiguousShortOptionName
        {
            [MappedOption(nameof(Message))] public string Message { get; set; }

            [MappedOption(nameof(Mode))] public string Mode { get; set; }
        }

        [Fact]
        public void ThrowsWhenShortOptionNamesAreAmbiguous()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Create<AmbiguousShortOptionName>());

            Assert.Equal(
                Strings.OptionNameIsAmbiguous("m",
                    typeof(AmbiguousShortOptionName).GetProperty("Mode"),
                    typeof(AmbiguousShortOptionName).GetProperty("Message")),
                ex.Message);
        }
        */

        /*
        private class AmbiguousLongOptionName
        {
            [Option("--no-edit")] public int ManuallySetToNoEdit { get; }

            [Option] public int NoEdit { get; }
        }

        [Fact]
        public void ThrowsWhenLongOptionNamesAreAmbiguous()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => Create<AmbiguousLongOptionName>());

            Assert.Equal(
                Strings.OptionNameIsAmbiguous("no-edit",
                    typeof(AmbiguousLongOptionName).GetProperty("NoEdit"),
                    typeof(AmbiguousLongOptionName).GetProperty("ManuallySetToNoEdit")),
                ex.Message);
        }
        */

        private class BothOptionAndArgument
        {
            [MappedOption(nameof(NotPossible))]
            [Argument(0)]
            public int NotPossible { get; }
        }

        [Fact]
        public void ThrowsWhenOptionAndArgumentAreSpecified()
        {
            FluentActions.Invoking(() => Create<BothOptionAndArgument>())
                .Should().Throw<InvalidOperationException>()
                .And
                .Message.Should().Be(Strings.BothOptionAndArgumentAttributesCannotBeSpecified(
                    typeof(BothOptionAndArgument).GetProperty("NotPossible")));
        }


        private class OptionHasDefaultValues
        {
            [MappedOption("-a1", "z")] public string Arg1 { get; } = "a";

            [MappedOption("-a2", new[] { "y" })] public string[] Arg2 { get; } = new[] { "b", "c" };

            [MappedOption("-a3", true)] public bool? Arg3 { get; }
        }

        [Fact]
        public void KeepsDefaultValues()
        {
            using var assertionScope = new AssertionScope();

            {
                var app1 = Create<OptionHasDefaultValues>();
                app1.Parse("-a1", "-a2");
                app1.Model.Arg1.Should().Be("z");
                app1.Model.Arg2.Should().BeEquivalentTo("y");
                app1.Model.Arg3.Should().NotHaveValue();
            }

            {
                var app2 = Create<OptionHasDefaultValues>();
                app2.Parse("-a1");
                app2.Model.Arg1.Should().Be("z");
                app2.Model.Arg2.Should().BeEquivalentTo("b", "c");
                app2.Model.Arg3.Should().NotHaveValue();
            }

            {
                var app3 = Create<OptionHasDefaultValues>();
                app3.Parse();
                app3.Model.Arg1.Should().Be("a");
                app3.Model.Arg2.Should().BeEquivalentTo("b", "c");
                app3.Model.Arg3.Should().NotHaveValue();
            }

            {
                var app4 = Create<OptionHasDefaultValues>();
                app4.Parse("-a3");
                app4.Model.Arg1.Should().Be("a");
                app4.Model.Arg2.Should().BeEquivalentTo("b", "c");
                app4.Model.Arg3.Should().BeTrue();
            }
        }

        private class PrivateSetterProgram
        {
            public int _value;
            public static int _staticvalue;

            [Option] public int Number { get; private set; }

            [Option] public int Count { get; }

            [Option]
            public int Value
            {
                get => _value;
                set => _value = value + 1;
            }

            [Option("--static-number")] public static int StaticNumber { get; private set; }

            [Option("--static-string")] public static string? StaticString { get; }

            [Option("--static-value")]
            public static int StaticValue
            {
                get => _staticvalue;
                set => _staticvalue = value + 1;
            }
        }

        [Fact]
        public void BindsToPrivateSetProperties()
        {
            var parsed = CommandLineParser.ParseArgs<PrivateSetterProgram>("--number", "1");
            Assert.Equal(1, parsed.Number);
        }

        [Fact]
        public void BindsToReadOnlyProperties()
        {
            var parsed = CommandLineParser.ParseArgs<PrivateSetterProgram>("--count", "1");
            Assert.Equal(1, parsed.Count);
        }

        [Fact]
        public void BindsToPropertiesWithSetterMethod()
        {
            var parsed = CommandLineParser.ParseArgs<PrivateSetterProgram>("--value", "1");
            Assert.Equal(2, parsed.Value);
        }

        [Fact]
        public void BindsToStaticProperties()
        {
            CommandLineParser.ParseArgs<PrivateSetterProgram>("--static-number", "1");
            Assert.Equal(1, PrivateSetterProgram.StaticNumber);
        }

#if !NETCOREAPP3_1 && !NET5_0
        // .NET Core 3.0 made an intentional breaking change
        // see https://github.com/dotnet/coreclr/issues/21268
        [Fact]
        public void BindsToStaticReadOnlyProps()
        {
            CommandLineParser.ParseArgs<PrivateSetterProgram>("--static-string", "1");
            Assert.Equal("1", PrivateSetterProgram.StaticString);
        }
#endif

        [Fact]
        public void BindsToStaticPropertiesWithSetterMethod()
        {
            CommandLineParser.ParseArgs<PrivateSetterProgram>("--static-value", "1");
            Assert.Equal(2, PrivateSetterProgram.StaticValue);
        }

        abstract class PrivateBaseType
        {
            [Option] int Count { get; }

            public int GetCount() => Count;
        }

        class WithPrivateBaseTypeApplication : PrivateBaseType
        {
        }

        [Fact]
        public void BindsToPrivateBaseTypeProperty()
        {
            var parsed = CommandLineParser.ParseArgs<WithPrivateBaseTypeApplication>("--count", "42");
            Assert.Equal(42, parsed.GetCount());
        }

        [Theory]
        [InlineData("Option123", "o", "option123", "OPTION123")]
        [InlineData("dWORD", "d", "d-word", "D_WORD")]
        [InlineData("MSBuild", "m", "msbuild", "MSBUILD")]
        [InlineData("NoEdit", "n", "no-edit", "NO_EDIT")]
        [InlineData("SetUpstreamBranch", "s", "set-upstream-branch", "SET_UPSTREAM_BRANCH")]
        [InlineData("lowerCaseFirst", "l", "lower-case-first", "LOWER_CASE_FIRST")]
        [InlineData("_field", "f", "field", "FIELD")]
        [InlineData("__field", "f", "field", "FIELD")]
        [InlineData("___field", "f", "field", "FIELD")]
        [InlineData("m_field", "m", "m-field", "M_FIELD")]
        [InlineData("m_Field", "m", "m-field", "M_FIELD")]
        public void ItDeterminesShortAndLongOptionNames(string propName, string shortName, string longName,
            string valueName)
        {
            var option = CreateOption(typeof(int), propName);
            Assert.Equal(longName, option.LongName);
            Assert.Equal(shortName, option.ShortName);
            Assert.Equal(valueName, option.ValueName);
        }

        private CommandOption CreateOption(Type propType, string propName)
        {
            // Use ref emit to generate a simple type with one property on it
            var assembly =
                AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Test"), AssemblyBuilderAccess.RunAndCollect);
            var mb = assembly.DefineDynamicModule("Test");
            var tb = mb.DefineType("Program");
            var pb = tb.DefineProperty(propName, PropertyAttributes.None, propType, Array.Empty<Type>());
            tb.DefineField($"<{propName}>k__BackingField", propType, FieldAttributes.Private);
            var ctor = typeof(OptionAttribute).GetConstructor(Array.Empty<Type>());
            var ab = new CustomAttributeBuilder(ctor, Array.Empty<object>());
            pb.SetCustomAttribute(ab);
            var program = tb.CreateType();
            var appBuilder = typeof(CommandLineApplication<>).MakeGenericType(program);
            var app = (CommandLineApplication)Activator.CreateInstance(appBuilder, Util.EmptyArray<object>());
            app.Conventions.UseOptionAttributes();
            return app.Options.OfType<CommandOption>().First();
        }
    }
}
