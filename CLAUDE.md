# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/claude-code) when working with this repository.

## Project Overview

**McMaster.Extensions.CommandLineUtils** is a .NET library for building command-line applications. It simplifies parsing command-line arguments, validating user inputs, and generating help text. This is a fork of Microsoft.Extensions.CommandLineUtils.

- **Package:** `McMaster.Extensions.CommandLineUtils` on NuGet
- **Targets:** .NET 6.0+
- **Status:** Stable/maintenance mode (no major features planned)
- **License:** Apache 2.0

## Build Commands

```bash
# Full build with tests (recommended)
./build.ps1

# CI mode (enforces formatting checks)
./build.ps1 -ci

# Build only
dotnet build

# Run tests
dotnet test

# Build sample projects
dotnet build docs/samples/samples.sln

# Generate documentation locally
./docs/generate.ps1
./docs/generate.ps1 -Serve  # with local preview
```

Build outputs (NuGet packages) go to `/artifacts/`.

## Project Structure

```
src/
  CommandLineUtils/           # Main library
    Attributes/               # Attribute-based API ([Option], [Argument], etc.)
    Conventions/              # Convention system for processing attributes
    Validation/               # Validation framework
    IO/                       # Console I/O abstractions (IConsole)
    HelpText/                 # Help text generation
    Internal/                 # Parsing engine, value parsers
    Utilities/                # ArgumentEscaper, Prompt, DotNetExe
  Hosting.CommandLine/        # Microsoft.Extensions.Hosting integration

test/
  CommandLineUtils.Tests/     # xUnit tests for main library
  Hosting.CommandLine.Tests/  # xUnit tests for hosting package

docs/
  samples/                    # 20+ example projects demonstrating various patterns
```

## Two APIs

**1. Attribute-based API:**
```csharp
public class Program
{
    public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    [Option(Description = "The subject")]
    public string Subject { get; } = "world";

    private void OnExecute() { /* command logic */ }
}
```

**2. Builder API:**
```csharp
var app = new CommandLineApplication();
app.HelpOption();
var subject = app.Option("-s|--subject <SUBJECT>", "desc", CommandOptionType.SingleValue);
app.OnExecute(() => { /* command logic */ });
return app.Execute(args);
```

## Key Patterns

- **Conventions:** Extensible `IConvention` implementations process attributes and wire up behavior
- **Validation:** Three approaches - attribute validators (`[Required]`, `[FileExists]`), fluent API (`.IsRequired()`, `.Accepts()`), and `OnValidate()` methods
- **Console abstraction:** `IConsole` interface enables testing; use `PhysicalConsole` for real I/O
- **Response files:** Support for `@filename` syntax to read arguments from files

## Code Style

- 4-space indentation
- Nullable reference types enabled
- Warnings treated as errors
- Code style enforced in build (`EnforceCodeStyleInBuild=true`)
- Format check: `dotnet tool run dotnet-format`

## Testing

Uses xUnit with FluentAssertions and Moq. Convention tests inherit from `ConventionTestBase` and use `Create<T>()` factory method.

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Development Workflow

**Test-Driven Development:** When implementing new features or fixing bugs, prefer writing tests first:

1. Write a failing test that demonstrates the desired behavior or reproduces the bug
2. Run the test to confirm it fails as expected
3. Implement the minimum code needed to make the test pass
4. Run tests to verify the fix
5. Refactor if needed while keeping tests green

This approach ensures code correctness, prevents regressions, and validates that tests actually catch the issues they're meant to detect. The test suite already has good coverage and patterns to follow.

## Claude Commands

Custom commands for Claude Code are in `.claude/commands/`:

- `/code-review <pr-number>` - Review pull request changes for substantive issues (bugs, security, correctness). Focuses on high-value feedback appropriate for open source contributions.
