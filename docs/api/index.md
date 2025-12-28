---
uid: latest_api_ref
---
API Reference
=============

**Version 5.0** (Current)

## Target Framework

McMaster.Extensions.CommandLineUtils targets **.NET 8.0**.

For older framework support, use previous versions:
 - Version 4.x: .NET 6.0+
 - Version 3.x: .NET Standard 2.0, .NET Framework 4.5

## Key Types

The main entry point for most command line applications is [CommandLineApplication](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplication).

For apps built using the attribute API, these are the most common attributes:

 - [CommandAttribute](xref:McMaster.Extensions.CommandLineUtils.CommandAttribute) - Marks a class as a command
 - [OptionAttribute](xref:McMaster.Extensions.CommandLineUtils.OptionAttribute) - Defines command-line options
 - [ArgumentAttribute](xref:McMaster.Extensions.CommandLineUtils.ArgumentAttribute) - Defines positional arguments
 - [SubcommandAttribute](xref:McMaster.Extensions.CommandLineUtils.SubcommandAttribute) - Defines subcommands
 - [HelpOptionAttribute](xref:McMaster.Extensions.CommandLineUtils.HelpOptionAttribute) - Adds --help option
 - [VersionOptionAttribute](xref:McMaster.Extensions.CommandLineUtils.VersionOptionAttribute) - Adds --version option

## Utilities

 - [Prompt](xref:McMaster.Extensions.CommandLineUtils.Prompt) - Interactive prompts for user input
 - [DotNetExe](xref:McMaster.Extensions.CommandLineUtils.DotNetExe) - Locate the dotnet executable
 - [ArgumentEscaper](xref:McMaster.Extensions.CommandLineUtils.ArgumentEscaper) - Escape arguments for shell execution
 - [IConsole](xref:McMaster.Extensions.CommandLineUtils.IConsole) - Abstraction for console I/O (useful for testing)
