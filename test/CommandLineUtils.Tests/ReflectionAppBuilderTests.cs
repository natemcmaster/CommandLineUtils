// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ReflectionAppBuilderTests
    {
        private readonly ITestOutputHelper _output;

        public ReflectionAppBuilderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Subcommand("add", typeof(AddCmd))]
        [Subcommand("rm", typeof(RemoveCmd))]
        private class MasterApp
        {
            public object Subcommand { get; set; }
        }

        private class AddCmd
        { }

        private class RemoveCmd
        { }

        [Fact]
        public void AddsSubcommands()
        {
            var builder = new ReflectionAppBuilder<MasterApp>();
            builder.Initialize();
            Assert.Collection(builder.App.Commands.OrderBy(c => c.Name),
                add =>
                {
                    Assert.Equal("add", add.Name);
                },
                rm =>
                {
                    Assert.Equal("rm", rm.Name);
                });
        }

        [Fact]
        public void BindsToSubcommandProperty()
        {
            var builder = new ReflectionAppBuilder<MasterApp>();
            var bound = builder.Bind(new TestConsole(_output), new[] { "add" });
            var master = Assert.IsType<MasterApp>(bound.Target);
            Assert.IsType<AddCmd>(master.Subcommand);
        }

        [Command(ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated, AllowArgumentSeparator = true, ThrowOnUnexpectedArgument = false)]
        private class ParsingOptions
        { }

        [Fact]
        public void HandlesParsingOptionsAttribute()
        {
            var builder = new ReflectionAppBuilder<ParsingOptions>();
            builder.Initialize();
            Assert.Equal(ResponseFileHandling.ParseArgsAsLineSeparated, builder.App.ResponseFileHandling);
            Assert.True(builder.App.AllowArgumentSeparator);
            Assert.False(builder.App.ThrowOnUnexpectedArgument);
        }

        private class AttributesNotUsedClass
        {
            public int OptionA { get; set; }
            public string OptionB { get; set; }
        }

        [Fact]
        public void AttributesAreRequired()
        {
            var builder = new ReflectionAppBuilder<AttributesNotUsedClass>();
            builder.Initialize();
            Assert.Empty(builder.App.Arguments);
            Assert.Empty(builder.App.Commands);
            Assert.Empty(builder.App.Options);
            Assert.Null(builder.App.OptionHelp);
            Assert.Null(builder.App.OptionVersion);
        }

        private class AppWithUnknownOptionType
        {
            [Option]
            public ReflectionAppBuilderTests Option { get; }
        }

        [Fact]
        public void ThrowsWhenOptionTypeCannotBeDetermined()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new ReflectionAppBuilder<AppWithUnknownOptionType>().Initialize());
            Assert.Equal(
                Strings.CannotDetermineOptionType(typeof(AppWithUnknownOptionType).GetProperty("Option")),
                ex.Message);
        }

        private class ShortNameOverride
        {
            [Option(ShortName = "d1")]
            public string Detail1 { get; }

            [Option(ShortName = "d2")]
            public string Detail2 { get; }
        }

        [Fact]
        public void CanOverrideShortNameOnOption()
        {
            var builder = new ReflectionAppBuilder<ShortNameOverride>();
            builder.Initialize();
            Assert.Contains(builder.App.Options, o => o.ShortName == "d1" && o.LongName == "detail1");
            Assert.Contains(builder.App.Options, o => o.ShortName == "d2" && o.LongName == "detail2");
        }

        private class AmbiguousShortOptionName
        {
            [Option]
            public int Message { get; }

            [Option]
            public int Mode { get; }
        }

        [Fact]
        public void ThrowsWhenShortOptionNamesAreAmbiguous()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new ReflectionAppBuilder<AmbiguousShortOptionName>().Initialize());

            Assert.Equal(
                Strings.OptionNameIsAmbiguous("m",
                    typeof(AmbiguousShortOptionName).GetProperty("Mode"),
                    typeof(AmbiguousShortOptionName).GetProperty("Message")),
                ex.Message);
        }

        private class AmbiguousLongOptionName
        {
            [Option("--no-edit")]
            public int ManuallySetToNoEdit { get; }

            [Option]
            public int NoEdit { get; }
        }

        [Fact]
        public void ThrowsWhenLongOptionNamesAreAmbiguous()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new ReflectionAppBuilder<AmbiguousLongOptionName>().Initialize());

            Assert.Equal(
                Strings.OptionNameIsAmbiguous("no-edit",
                    typeof(AmbiguousLongOptionName).GetProperty("NoEdit"),
                    typeof(AmbiguousLongOptionName).GetProperty("ManuallySetToNoEdit")),
                ex.Message);
        }

        private class BothOptionAndArgument
        {
            [Option]
            [Argument(0)]
            public int NotPossible { get; }
        }

        [Fact]
        public void ThrowsWhenOptionAndArgumentAreSpecified()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new ReflectionAppBuilder<BothOptionAndArgument>().Initialize());

            Assert.Equal(
                Strings.BothOptionAndArgumentAttributesCannotBeSpecified(
                    typeof(BothOptionAndArgument).GetProperty("NotPossible")),
                ex.Message);
        }

        private class DuplicateArguments
        {
            [Argument(0)]
            public string First { get; }

            [Argument(0)]
            public int AlsoFirst { get; }
        }

        [Fact]
        public void ThrowsWhenDuplicateArgumentPositionsAreSpecified()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new ReflectionAppBuilder<DuplicateArguments>().Initialize());

            Assert.Equal(
                Strings.DuplicateArgumentPosition(
                    0,
                    typeof(DuplicateArguments).GetProperty("AlsoFirst"),
                    typeof(DuplicateArguments).GetProperty("First")),
                ex.Message);
        }

        private class MultipleValuesMultipleArgs
        {
            [Argument(0)]
            public string[] Words { get; }

            [Argument(1)]
            public string[] MoreWords { get; }
        }

        [Fact]
        public void ThrowsWhenMultipleArgumentsAllowMultipleValues()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new ReflectionAppBuilder<MultipleValuesMultipleArgs>().Initialize());

            Assert.Equal(
                Strings.OnlyLastArgumentCanAllowMultipleValues("Words"),
                ex.Message);
        }

        private class SimpleProgram
        {
            [Argument(0)]
            public string Command { get; set; }

            [Option]
            public string Message { get; set; }

            [Option("-F <file>")]
            public string File { get; set; }

            [Option]
            public bool Amend { get; set; }

            [Option("--no-edit")]
            public bool NoEdit { get; set; }
        }

        [Fact]
        public void InitializesTypeWithAttributes()
        {
            var program = CommandLineParser.ParseArgs<SimpleProgram>("commit", "-m", "Add attribute parsing", "--amend");

            Assert.NotNull(program);
            Assert.Equal("commit", program.Command);
            Assert.Equal("Add attribute parsing", program.Message);
            Assert.True(program.Amend);
            Assert.False(program.NoEdit);
        }

        [Fact]
        public void ThrowsForArgumentsWithoutMatchingAttribute()
        {
            var ex = Assert.Throws<CommandParsingException>(
                () => CommandLineParser.ParseArgs<SimpleProgram>("-f"));
            Assert.StartsWith("Unrecognized option '-f'", ex.Message);
        }

        [Command(ThrowOnUnexpectedArgument = false)]
        private class RemainingArgs_Array
        {
            public string[] RemainingArguments { get; }
        }

        [Fact]
        public void ItSetsRemainingArguments_Array()
        {
            var result = CommandLineParser.ParseArgs<RemainingArgs_Array>("a", "b");
            Assert.Equal(new[] { "a", "b" }, result.RemainingArguments);
        }

        [Command(ThrowOnUnexpectedArgument = false)]
        private class RemainingArgs_List
        {
            public List<string> RemainingArguments { get; }
        }

        [Fact]
        public void ItSetsRemainingArguments_List()
        {
            var result = CommandLineParser.ParseArgs<RemainingArgs_List>("a", "b");
            Assert.Equal(new[] { "a", "b" }, result.RemainingArguments);
        }

        private class PrivateSetterProgram
        {
            public int _value;
            public static int _staticvalue;

            [Option]
            public int Number { get; private set; }

            [Option]
            public int Count { get; }

            [Option]
            public int Value
            {
                get => _value;
                set => _value = value + 1;
            }

            [Option("--static-number")]
            public static int StaticNumber { get; private set; }

            [Option("--static-string")]
            public static string StaticString { get; }

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

        [Fact]
        public void BindsToStaticReadOnlyProps()
        {
            CommandLineParser.ParseArgs<PrivateSetterProgram>("--static-string", "1");
            Assert.Equal("1", PrivateSetterProgram.StaticString);
        }

        [Fact]
        public void BindsToStaticPropertiesWithSetterMethod()
        {
            CommandLineParser.ParseArgs<PrivateSetterProgram>("--static-value", "1");
            Assert.Equal(2, PrivateSetterProgram.StaticValue);
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
        public void ItDeterminesShortAndLongOptionNames(string propName, string shortName, string longName, string valueName)
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
            var fb = tb.DefineField($"<{propName}>k__BackingField", propType, FieldAttributes.Private);
            var ctor = typeof(OptionAttribute).GetConstructor(Array.Empty<Type>());
            var ab = new CustomAttributeBuilder(ctor, Array.Empty<object>());
            pb.SetCustomAttribute(ab);
            var program = tb.CreateTypeInfo();
            var appBuilder = typeof(ReflectionAppBuilder<>).MakeGenericType(program.AsType());
            var instance = Activator.CreateInstance(appBuilder);
            var init = appBuilder.GetTypeInfo().GetDeclaredMethod("Initialize");
            init.Invoke(instance, Constants.EmptyArray);
            var getter = appBuilder.GetTypeInfo().GetDeclaredProperty("App").GetMethod;
            var app = (CommandLineApplication)getter.Invoke(instance, Array.Empty<object>());
            return app.Options[0];
        }
    }
}
