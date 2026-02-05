// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;

// Test assembly is not strong-named to avoid .NET Framework strong name verification issues
[assembly: InternalsVisibleTo("McMaster.Extensions.CommandLineUtils.Tests")]
