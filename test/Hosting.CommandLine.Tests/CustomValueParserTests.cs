using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Conventions;
using McMaster.Extensions.Hosting.CommandLine.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.Extensions.Hosting.CommandLine.Tests
{
    public class CustomValueParserTests
    {
        private const string DemoOptionValue = "{\"Value\": \"TheValue\"}";

        private readonly ITestOutputHelper _output;

        public CustomValueParserTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task ItParsesUsingCustomParserFromConfigAction()
        {
            var exitCode = await new HostBuilder()
                .ConfigureServices(collection => collection.AddSingleton<IConsole>(new TestConsole(_output)))
                .RunCommandLineApplicationAsync<CustomOptionTypeCommand>(
                    new[] { "--custom-type", DemoOptionValue },
                    app => app.ValueParsers.AddOrReplace(
                        new CustomValueParser()));
            Assert.Equal(0, exitCode);
        }

        [Fact]
        public async Task ItParsesUsingCustomParserFromInjectedConvention()
        {
            var exitCode = await new HostBuilder()
                .ConfigureServices(collection =>
                {
                    collection.AddSingleton<IConsole>(new TestConsole(_output));
                    collection.AddSingleton<IConvention, CustomValueParserConvention>();
                })
                .RunCommandLineApplicationAsync<CustomOptionTypeCommand>(
                    new[] { "--custom-type", DemoOptionValue });
            Assert.Equal(0, exitCode);
        }

        [Fact]
        public async Task ItParsesUsingCustomParserFromAttribute()
        {
            var exitCode = await new HostBuilder()
                .ConfigureServices(collection => collection.AddSingleton<IConsole>(new TestConsole(_output)))
                .RunCommandLineApplicationAsync<CustomOptionTypeCommandWithAttribute>(
                    new[] { "--custom-type", DemoOptionValue });
            Assert.Equal(0, exitCode);
        }

        class CustomType
        {
            public string Value { get; set; }
        }

        class CustomValueParser : IValueParser<CustomType>
        {
            public Type TargetType => typeof(CustomType);

            public CustomType Parse(string? argName, string? value, CultureInfo culture)
            {
                return JsonSerializer.Deserialize<CustomType>(value);
            }

            object? IValueParser.Parse(string? argName, string? value, CultureInfo culture)
            {
                return Parse(argName, value, culture);
            }
        }

        [Command]
        class CustomOptionTypeCommand
        {
            [Option("--custom-type", CommandOptionType.SingleValue)]
            public CustomType Option { get; set; }

            private int OnExecute()
            {
                if (Option == null)
                {
                    return 1;
                }

                if (!"TheValue".Equals(Option.Value, StringComparison.Ordinal))
                {
                    return 2;
                }

                return 0;
            }
        }

        class CustomValueParserConvention : IConvention
        {
            public void Apply(ConventionContext context)
            {
                context.Application.ValueParsers.AddOrReplace(new CustomValueParser());
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        class CustomValueParserConventionAttribute : Attribute, IConvention
        {
            public void Apply(ConventionContext context)
            {
                context.Application.ValueParsers.AddOrReplace(new CustomValueParser());
            }
        }

        [Command]
        [CustomValueParserConvention]
        class CustomOptionTypeCommandWithAttribute
        {
            [Option("--custom-type", CommandOptionType.SingleValue)]
            public CustomType Option { get; set; }

            private int OnExecute()
            {
                if (Option == null)
                {
                    return 1;
                }

                if (!"TheValue".Equals(Option.Value, StringComparison.Ordinal))
                {
                    return 2;
                }

                return 0;
            }
        }
    }
}
