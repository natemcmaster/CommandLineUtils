// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;

namespace AotSample;

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
[Subcommand(typeof(GreetCommand), typeof(InfoCommand))]
public class Program
{
    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    [Option("-v|--verbose", Description = "Enable verbose output")]
    public bool Verbose { get; set; }

    internal int OnExecute(CommandLineApplication app)
    {
        app.ShowHelp();
        return 0;
    }
}

[Command(Name = "greet", Description = "Greet someone")]
public class GreetCommand
{
    [Argument(0, Name = "name", Description = "The name of the person to greet")]
    [Required]
    public string Name { get; set; } = "";

    [Option("-l|--loud", Description = "Use uppercase")]
    public bool Loud { get; set; }

    internal int OnExecute()
    {
        var greeting = $"Hello, {Name}!";
        if (Loud)
        {
            greeting = greeting.ToUpperInvariant();
        }
        Console.WriteLine(greeting);
        return 0;
    }
}

[Command(Name = "info", Description = "Display runtime information")]
public class InfoCommand
{
    internal int OnExecute()
    {
        Console.WriteLine("AOT Sample Application");
        Console.WriteLine($"  Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"  OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
        Console.WriteLine($"  Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");

        // Check if we have generated metadata
        var hasGenerated = McMaster.Extensions.CommandLineUtils.SourceGeneration
            .CommandMetadataRegistry.HasMetadata(typeof(Program));
        Console.WriteLine($"  Generated Metadata: {hasGenerated}");

        return 0;
    }
}
