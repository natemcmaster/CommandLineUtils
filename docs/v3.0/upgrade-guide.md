---
uid: 2.x-to-3.0-upgrade
---
# Upgrading to CommandLineUtils 3.0

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

