---
uid: 2.x-to-3.0-upgrade
---
# Upgrading to CommandLineUtils 3.0

For more technical details, see [this list of GitHub issues](https://github.com/natemcmaster/CommandLineUtils/issues?q=label%3Abreaking-change+milestone%3A3.0).

## Breaking API changes

3.0 removed API that were in the 2.x version of this library. All API that was removed should have been marked
as `[Obsolete]` in a 2.x release.

### Symptom

After updating to 3.0, code doesn't compile with errors like

> error CS1061: 'CommandLineApplication' does not contain a definition for ....something.... (are you missing a using directive or an assembly reference?)

### Resolution

If you were using 2.x before, you should have received compiler warnings like

> warning CS0618: ABC is obsolete: 'This method has been marked as obsolete and will be removed in a future version. The recommended replacement is XYZ.

Follow these recommendations to update to the new API.

## NuGet compatibility with older platforms

3.0 removed support for older .NET platforms, like .NET Standard 1.6, .NET Core 1.x, and UWP 8.0. The library still supports .NET Framework 4.5 and .NET Standard 2.0.

### Symptom

NuGet fails to install your project with an error like

> error NU1202: Package McMaster.Extensions.CommandLineUtils 3.0.0 is not compatible with netcoreapp1.1 (.NETCoreApp,Version=v1.1). Package McMaster.Extensions.CommandLineUtils 3.1.0 supports:
> error NU1202:   - net45 (.NETFramework,Version=v4.5)
> error NU1202:   - netstandard2.0 (.NETStandard,Version=v2.0)

### Resolution

Either keep using CommandLineUtils 2.x, or upgrade your application to something newer. See https://dotnet.microsoft.com/platform/dotnet-standard for a list of .NET platforms compatible with .NET Standard 2.0.

## Upgrading McMaster.Extensions.Hosting.CommandLine

In order to fix [#294], McMaster.Extensions.Hosting.CommandLine 3.0's dependency on Microsoft.Extensions.Hosting
was lowered to a dependency on Microsoft.Extensions.Hosting.**Abstractions**. In some cases, this could
cause your app to fail to compile when you upgrade with errors.

[#294]: https://github.com/natemcmaster/CommandLineUtils/issues/294

### Symptom

After upgrading to 3.0, your app fails to compile with

> error CS0246: The type or namespace name 'HostBuilder' could not be found"

### Resolution

Add a dependency on [Microsoft.Extensions.Hosting](https://nuget.org/packages/Microsoft.Extensions.Hosting)

