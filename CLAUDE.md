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
# Quick test run (may not catch all issues)
dotnet test --collect:"XPlat Code Coverage"

# Full validation (REQUIRED before committing)
pwsh -File build.ps1
```

**IMPORTANT:** Always run the full `build.ps1` script before committing changes. `dotnet test` alone may pass while the full build fails due to:
- Sample project compilation issues
- Source generator output problems
- Integration test failures
- Code coverage requirements

The build script runs the complete validation pipeline including tests, samples, and packaging.

## Development Workflow

**Test-Driven Development:** When implementing new features or fixing bugs, prefer writing tests first:

1. Write a failing test that demonstrates the desired behavior or reproduces the bug
2. Run the test to confirm it fails as expected
3. Implement the minimum code needed to make the test pass
4. Run the **full build script** (`pwsh -File build.ps1`) to verify the fix
5. Refactor if needed while keeping tests green

This approach ensures code correctness, prevents regressions, and validates that tests actually catch the issues they're meant to detect. The test suite already has good coverage and patterns to follow.

## Commit Guidelines

**IMPORTANT:** Use Conventional Commit format for all commit messages. This ensures consistency and enables automated changelog generation.

**Format:** `<type>[optional scope]: <description>`

**Common types:**
- `feat:` New feature or enhancement
- `fix:` Bug fix
- `docs:` Documentation changes only
- `test:` Adding or updating tests
- `refactor:` Code changes that neither fix bugs nor add features
- `perf:` Performance improvements
- `chore:` Build scripts, dependencies, tooling

**Examples:**
```
feat: add support for nested subcommands
fix: resolve null reference in argument parser
docs: update getting started guide
test: add coverage for validation attributes
refactor: simplify help text generation logic
```

**Multi-line commits:** For complex changes, use a blank line followed by a detailed body:
```
fix: resolve race condition in async command execution

The ExecuteAsync method was not properly awaiting disposal of
resources, leading to intermittent failures in concurrent scenarios.
Added proper async/await pattern and additional test coverage.
```

**Skipping CI:** For commits that don't require CI validation (documentation, README updates, comment changes), add `[ci skip]` on its own line in the commit body. **IMPORTANT:** Never include `[ci skip]` in the first line of the commit message.

```
docs: update installation instructions

[ci skip]
```

```
docs: fix typo in API documentation

[ci skip]
```

This prevents unnecessary CI builds and saves resources for changes that don't affect code functionality.

## Release Management

Release notes are managed in two places:
1. **`src/CommandLineUtils/releasenotes.props`** - XML format for NuGet's `<PackageReleaseNotes>`
2. **`CHANGELOG.md`** - Markdown format for GitHub

The `/prepare-release` skill automates release note generation by analyzing git history and formatting changes appropriately.
