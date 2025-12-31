// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;

namespace AotSample
{

    /// <summary>
    /// This sample demonstrates AOT-compatible command-line application usage.
    ///
    /// When the source generator is active (which it is when referencing the main library),
    /// it automatically generates ICommandMetadataProvider implementations for all [Command] classes.
    /// This allows the application to work without runtime reflection, enabling Native AOT compilation.
    ///
    /// To publish as a native AOT executable:
    ///   dotnet publish -c Release
    /// </summary>
    [Command(Name = "aot-sample", Description = "An AOT-compatible CLI sample")]
    [HelpOption]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [Subcommand(typeof(GreetCommand), typeof(InfoCommand), typeof(EchoCommand), typeof(DiCommand), typeof(ShowVersionCommand))]
    public class Program
    {
        public static int Main(string[] args)
        {
            // Create a simple service provider for DI testing
            var services = new SimpleServiceProvider();
            services.Register<ILogger>(new ConsoleLogger());

            var app = new CommandLineApplication<Program>();
            app.Conventions.UseDefaultConventions().UseConstructorInjection(services);
            return app.Execute(args);
        }

        /// <summary>
        /// Gets the version string dynamically.
        /// </summary>
        public string GetVersion => "2.0.0-dynamic";

        [Option("-v|--verbose", Description = "Enable verbose output")]
        public bool Verbose { get; set; }

        /// <summary>
        /// The selected subcommand (set by convention).
        /// </summary>
        public object? Subcommand { get; set; }

        internal int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 0;
        }

    }

}
