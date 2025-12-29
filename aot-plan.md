# AOT Compatibility Plan for McMaster.Extensions.CommandLineUtils

## Summary

Replace runtime reflection with compile-time source generators to enable Native AOT compilation for applications using the attribute-based API.

**Target:** .NET 8+ only
**Compatibility:** Dual-mode (reflection fallback for non-AOT scenarios)
**Scope:** Full feature parity
**Opt-in:** Automatic detection (generator runs on any `[Command]` class)

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    User's Command Class                          │
│   [Command] class MyCmd { [Option] string Name; OnExecute(); }  │
└─────────────────────────────────────────────────────────────────┘
                              │
          ┌───────────────────┴───────────────────┐
          ▼                                       ▼
┌──────────────────────┐              ┌──────────────────────────┐
│   Source Generator   │              │   Reflection Path        │
│   (compile-time)     │              │   (runtime fallback)     │
└──────────────────────┘              └──────────────────────────┘
          │                                       │
          ▼                                       ▼
┌──────────────────────┐              ┌──────────────────────────┐
│ Generated Metadata   │              │ ReflectionMetadataProvider│
│ Provider + Registry  │              │ (existing conventions)   │
└──────────────────────┘              └──────────────────────────┘
          │                                       │
          └───────────────────┬───────────────────┘
                              ▼
              ┌───────────────────────────────────┐
              │  ICommandMetadataProvider         │
              │  (common abstraction layer)       │
              └───────────────────────────────────┘
                              │
                              ▼
              ┌───────────────────────────────────┐
              │  Modified Conventions             │
              │  (use metadata, not reflect)      │
              └───────────────────────────────────┘
```

---

## New Project Structure

```
src/
  CommandLineUtils/
    SourceGeneration/                    # NEW: Abstraction layer
      ICommandMetadataProvider.cs        # Core metadata interface
      CommandMetadataRegistry.cs         # Static registry for generated providers
      IMetadataResolver.cs               # Bridge between generated/reflection
      ReflectionMetadataProvider.cs      # Fallback using reflection
      Metadata/
        OptionMetadata.cs
        ArgumentMetadata.cs
        SubcommandMetadata.cs
        CommandMetadata.cs
      Handlers/
        IExecuteHandler.cs
        IValidateHandler.cs
        IModelFactory.cs

  CommandLineUtils.Generators/          # NEW: Source generator project
    McMaster.Extensions.CommandLineUtils.Generators.csproj
    CommandGenerator.cs                  # Incremental generator entry point
    Analyzers/
      CommandModelAnalyzer.cs            # Extracts metadata from syntax
    Emitters/
      CommandConfiguratorEmitter.cs      # Generates configurator code
      MetadataProviderEmitter.cs         # Generates ICommandMetadataProvider impl
```

---

## Implementation Phases

### Phase 1: Abstraction Layer (Foundation)

Create the metadata interfaces that both reflection and generated code will implement.

**Files to create:**
- `src/CommandLineUtils/SourceGeneration/ICommandMetadataProvider.cs`
- `src/CommandLineUtils/SourceGeneration/CommandMetadataRegistry.cs`
- `src/CommandLineUtils/SourceGeneration/Metadata/*.cs`
- `src/CommandLineUtils/SourceGeneration/Handlers/*.cs`

**Key interface:**
```csharp
public interface ICommandMetadataProvider
{
    Type ModelType { get; }
    IReadOnlyList<OptionMetadata> Options { get; }
    IReadOnlyList<ArgumentMetadata> Arguments { get; }
    IReadOnlyList<SubcommandMetadata> Subcommands { get; }
    CommandMetadata? CommandInfo { get; }
    IExecuteHandler? ExecuteHandler { get; }
    IValidateHandler? ValidateHandler { get; }
    IModelFactory GetModelFactory(IServiceProvider? services);
}
```

**Registry pattern:**
```csharp
public static class CommandMetadataRegistry
{
    private static readonly ConcurrentDictionary<Type, ICommandMetadataProvider> _providers = new();

    public static void Register<T>(ICommandMetadataProvider<T> provider) where T : class
        => _providers[typeof(T)] = provider;

    public static bool TryGetProvider(Type type, out ICommandMetadataProvider? provider)
        => _providers.TryGetValue(type, out provider);
}
```

### Phase 2: Reflection Wrapper

Wrap existing reflection logic in `ICommandMetadataProvider` for fallback.

**Files to create:**
- `src/CommandLineUtils/SourceGeneration/ReflectionMetadataProvider.cs`
- `src/CommandLineUtils/SourceGeneration/DefaultMetadataResolver.cs`

**Key implementation:**
```csharp
[RequiresUnreferencedCode("Uses reflection to analyze the model type")]
internal sealed class ReflectionMetadataProvider : ICommandMetadataProvider
{
    // Wraps existing ReflectionHelper and convention logic
    // Provides same metadata via reflection when generator hasn't run
}

public sealed class DefaultMetadataResolver : IMetadataResolver
{
    public ICommandMetadataProvider GetProvider(Type modelType)
    {
        // Check registry first (generated), fall back to reflection
        if (CommandMetadataRegistry.TryGetProvider(modelType, out var provider))
            return provider;
        return new ReflectionMetadataProvider(modelType);
    }
}
```

### Phase 3: Convention Modifications

Modify existing conventions to use `ICommandMetadataProvider` instead of direct reflection.

**Files to modify:**
- `src/CommandLineUtils/Conventions/ExecuteMethodConvention.cs`
- `src/CommandLineUtils/Conventions/ValidateMethodConvention.cs`
- `src/CommandLineUtils/Conventions/OptionAttributeConvention.cs`
- `src/CommandLineUtils/Conventions/ArgumentAttributeConvention.cs`
- `src/CommandLineUtils/Conventions/SubcommandAttributeConvention.cs`
- `src/CommandLineUtils/Conventions/ConstructorInjectionConvention.cs`

**Example modification (ExecuteMethodConvention.cs):**
```csharp
public class ExecuteMethodConvention : IConvention
{
    public virtual void Apply(ConventionContext context)
    {
        if (context.ModelType == null) return;
        context.Application.OnExecuteAsync(async ct => await OnExecute(context, ct));
    }

    private async Task<int> OnExecute(ConventionContext context, CancellationToken ct)
    {
        var resolver = DefaultMetadataResolver.Instance;
        var provider = resolver.GetProvider(context.ModelType!);

        if (provider.ExecuteHandler != null)
        {
            // Use generated/resolved handler - no reflection!
            return await provider.ExecuteHandler.InvokeAsync(
                context.ModelAccessor!.GetModel(),
                context.Application,
                ct);
        }

        throw new InvalidOperationException(Strings.NoOnExecuteMethodFound);
    }
}
```

### Phase 4: Source Generator Project

Create the incremental source generator.

**Files to create:**
- `src/CommandLineUtils.Generators/McMaster.Extensions.CommandLineUtils.Generators.csproj`
- `src/CommandLineUtils.Generators/CommandGenerator.cs`
- `src/CommandLineUtils.Generators/Analyzers/CommandModelAnalyzer.cs`
- `src/CommandLineUtils.Generators/Emitters/MetadataProviderEmitter.cs`

**Generator project file:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>12.0</LangVersion>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
    <IsRoslynComponent>true</IsRoslynComponent>
    <IncludeBuildOutput>false</IncludeBuildOutput>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup>
    <None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" />
  </ItemGroup>
</Project>
```

**Incremental generator entry point:**
```csharp
[Generator(LanguageNames.CSharp)]
public class CommandGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find classes with [Command] attribute
        var commandClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "McMaster.Extensions.CommandLineUtils.CommandAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => CommandModelAnalyzer.Analyze(ctx));

        // Also detect classes used with Execute<T>()
        var executeTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsExecuteInvocation,
                transform: ExtractTypeArgument);

        // Generate metadata providers
        context.RegisterSourceOutput(commandClasses.Collect(), GenerateMetadataProviders);

        // Generate registry initialization
        context.RegisterSourceOutput(commandClasses.Collect(), GenerateRegistryInitializer);
    }
}
```

### Phase 5: Generated Code Structure

**For user's command:**
```csharp
[Command("greet")]
public class GreetCommand
{
    [Option("-n|--name")] public string Name { get; set; } = "World";
    [Argument(0)] public string Message { get; set; }
    public int OnExecute(IConsole console) => /* ... */;
}
```

**Generator produces:**
```csharp
// <auto-generated />
namespace MyApp.Generated
{
    [ModuleInitializer]
    internal static class CommandMetadataRegistration
    {
        public static void Initialize()
        {
            CommandMetadataRegistry.Register<GreetCommand>(new GreetCommandMetadataProvider());
        }
    }

    internal sealed class GreetCommandMetadataProvider : ICommandMetadataProvider<GreetCommand>
    {
        public Type ModelType => typeof(GreetCommand);

        public IReadOnlyList<OptionMetadata> Options { get; } = new[]
        {
            new OptionMetadata
            {
                PropertyName = "Name",
                PropertyType = typeof(string),
                Template = "-n|--name",
                OptionType = CommandOptionType.SingleValue,
                Getter = static obj => ((GreetCommand)obj).Name,
                Setter = static (obj, val) => ((GreetCommand)obj).Name = (string)val!
            }
        };

        public IReadOnlyList<ArgumentMetadata> Arguments { get; } = new[]
        {
            new ArgumentMetadata
            {
                PropertyName = "Message",
                PropertyType = typeof(string),
                Order = 0,
                Getter = static obj => ((GreetCommand)obj).Message,
                Setter = static (obj, val) => ((GreetCommand)obj).Message = (string)val!
            }
        };

        public IExecuteHandler ExecuteHandler { get; } = new GreetCommandExecuteHandler();

        public IModelFactory<GreetCommand> GetModelFactory(IServiceProvider? _)
            => new GreetCommandFactory();
    }

    internal sealed class GreetCommandExecuteHandler : IExecuteHandler
    {
        public bool IsAsync => false;

        public Task<int> InvokeAsync(object model, CommandLineApplication app, CancellationToken ct)
        {
            var cmd = (GreetCommand)model;
            var console = app._context.Console;
            return Task.FromResult(cmd.OnExecute(console));
        }
    }

    internal sealed class GreetCommandFactory : IModelFactory<GreetCommand>
    {
        public GreetCommand Create() => new GreetCommand();
        object IModelFactory.Create() => Create();
    }
}
```

### Phase 6: Trimmer Annotations

Add annotations to warn when reflection paths are used.

**Files to modify:**
- `src/CommandLineUtils/Internal/ReflectionHelper.cs` - Add `[RequiresUnreferencedCode]`
- `src/CommandLineUtils/CommandLineApplication.Execute.cs` - Add trimmer warnings
- `src/CommandLineUtils/Conventions/*.cs` - Annotate reflection usage

**Add to main library csproj:**
```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
  <EnableAotAnalyzer>true</EnableAotAnalyzer>
</PropertyGroup>
```

### Phase 7: Testing & Samples

**New test projects:**
- `test/CommandLineUtils.Generators.Tests/` - Generator unit tests
- `test/CommandLineUtils.Aot.Tests/` - AOT integration tests (PublishAot)

**Sample updates:**
- Add AOT-compatible sample project
- Update existing samples to verify they still work

---

## Critical Files to Modify

| File | Change Type | Description |
|------|-------------|-------------|
| `src/CommandLineUtils/CommandLineApplication{T}.cs:86` | Modify | Replace `Activator.CreateInstance<T>()` with factory |
| `src/CommandLineUtils/Conventions/ExecuteMethodConvention.cs:38-39` | Modify | Use `IExecuteHandler` instead of `GetMethod()` |
| `src/CommandLineUtils/Conventions/SubcommandAttributeConvention.cs` | Modify | Remove `MakeGenericMethod()` usage |
| `src/CommandLineUtils/Conventions/ConstructorInjectionConvention.cs` | Modify | Use generated factory |
| `src/CommandLineUtils/Internal/ReflectionHelper.cs` | Annotate | Add `[RequiresUnreferencedCode]` |
| `src/CommandLineUtils/Abstractions/ValueParserProvider.cs` | Modify | Remove `MakeGenericMethod()` |

---

## Reflection APIs to Replace

| Current Reflection | Generated Replacement |
|--------------------|----------------------|
| `GetMethod("OnExecute")` | `IExecuteHandler.InvokeAsync()` |
| `GetMethod("OnValidate")` | `IValidateHandler.Invoke()` |
| `GetProperties()` + `GetCustomAttribute<OptionAttribute>()` | `ICommandMetadataProvider.Options` |
| `Activator.CreateInstance<T>()` | `IModelFactory<T>.Create()` |
| `ConstructorInfo.Invoke()` | Generated factory with `new T(dep1, dep2)` |
| `MakeGenericMethod()` | Pre-generated typed methods |
| `PropertyInfo.GetValue/SetValue` | Generated `Getter`/`Setter` delegates |

---

## Deliverables Checklist

- [ ] `ICommandMetadataProvider` interface and related types
- [ ] `CommandMetadataRegistry` static registry
- [ ] `ReflectionMetadataProvider` fallback implementation
- [ ] Modified conventions using metadata abstraction
- [ ] `CommandLineUtils.Generators` project
- [ ] Incremental source generator implementation
- [ ] Module initializer for registry population
- [ ] Trimmer annotations on reflection paths
- [ ] Unit tests for generator
- [ ] AOT integration tests
- [ ] Migration documentation

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Generator complexity | High dev effort | Start with core features, iterate |
| Breaking changes | User migration | Dual-mode ensures backward compat |
| Partial types conflict | Compile errors | Use separate generated classes |
| Complex type hierarchies | Edge cases | Handle inheritance in analyzer |
| DI edge cases | Runtime errors | Match existing ConstructorInjectionConvention behavior |

---

## Success Criteria

1. Existing apps compile and run without changes (reflection fallback)
2. Apps with generator can `PublishAot` successfully
3. No runtime reflection in generated path
4. All existing tests pass
5. AOT-specific tests validate trimming works
