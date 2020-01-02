---
uid: latest_api_ref
---
API Reference
=============

**Version 2.4**

McMaster.Extensions.CommandLineUtils supports three target frameworks.

 - .NET Standard 2.0
 - .NET Standard 1.6
 - .NET Framework 4.5

The API is almost identical between all of the frameworks.

The main entry point for most command line applications is [CommandLineApplication](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplication).

For apps built using attributes, these are the most common attributes used:

 - [OptionAttribute](xref:McMaster.Extensions.CommandLineUtils.OptionAttribute)
 - [ArgumentAttribute](xref:McMaster.Extensions.CommandLineUtils.ArgumentAttribute)
 - [CommandAttribute](xref:McMaster.Extensions.CommandLineUtils.CommandAttribute)
 - [SubcommandAttribute](xref:McMaster.Extensions.CommandLineUtils.SubcommandAttribute)
 - [HelpOptionAttribute](xref:McMaster.Extensions.CommandLineUtils.HelpOptionAttribute)

Other commonly used types include

 - [DotNetExe](xref:McMaster.Extensions.CommandLineUtils.DotNetExe)
 - [Prompt](xref:McMaster.Extensions.CommandLineUtils.Prompt)
 - [ArgumentEscaper](xref:McMaster.Extensions.CommandLineUtils.ArgumentEscaper)
 - [IConsole](xref:McMaster.Extensions.CommandLineUtils.IConsole)
