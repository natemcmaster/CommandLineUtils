// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        [Command(HandleResponseFiles = true, AllowArgumentSeparator = true, ThrowOnUnexpectedArgument = false)]
        private class ParsingOptions
        { }

        [Fact]
        public void HandlesParsingOptionsAttribute()
        {
            var builder = new ReflectionAppBuilder<ParsingOptions>();
            builder.Initialize();
            Assert.True(builder.App.HandleResponseFiles);
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

        [Theory]
        [InlineData("Option123", "o", "option123")]
        [InlineData("dWORD", "d", "d-word")]
        [InlineData("MSBuild", "m", "msbuild")]
        [InlineData("NoEdit", "n", "no-edit")]
        [InlineData("SetUpstreamBranch", "s", "set-upstream-branch")]
        [InlineData("lowerCaseFirst", "l", "lower-case-first")]
        [InlineData("_field", "f", "field")]
        [InlineData("__field", "f", "field")]
        [InlineData("___field", "f", "field")]
        [InlineData("m_field", "m", "m-field")]
        [InlineData("m_Field", "m", "m-field")]
        public void ItDeterminesShortAndLongOptionNames(string propName, string shortName, string longName)
        {
            var option = CreateOption(typeof(int), propName);
            Assert.Equal(longName, option.LongName);
            Assert.Equal(shortName, option.ShortName);
            Assert.Equal(propName, option.ValueName);
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
