# PR Comment for #560: Keyed DI Support via Reflection

Thanks for adding keyed DI support! However, this introduces a hard dependency on `Microsoft.Extensions.DependencyInjection.Abstractions` in the core `CommandLineUtils` package. This could be avoided using reflection, which would:

1. Keep the package lighter for users who don't need keyed DI
2. Maintain compatibility with older DI container versions
3. Follow the existing pattern where the core library only depends on `IServiceProvider`

Here's how the `BindParameters` method could be rewritten to use reflection instead:

```csharp
// Cache these at class level for performance
private static readonly Type? s_fromKeyedServicesAttributeType;
private static readonly PropertyInfo? s_keyProperty;
private static readonly Type? s_keyedServiceProviderType;
private static readonly MethodInfo? s_getKeyedServiceMethod;

static ReflectionHelper()
{
    // Try to load keyed services types via reflection (available in .NET 8+)
    s_fromKeyedServicesAttributeType = Type.GetType(
        "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute, Microsoft.Extensions.DependencyInjection.Abstractions");

    if (s_fromKeyedServicesAttributeType != null)
    {
        s_keyProperty = s_fromKeyedServicesAttributeType.GetProperty("Key");
    }

    s_keyedServiceProviderType = Type.GetType(
        "Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider, Microsoft.Extensions.DependencyInjection.Abstractions");

    if (s_keyedServiceProviderType != null)
    {
        s_getKeyedServiceMethod = s_keyedServiceProviderType.GetMethod(
            "GetKeyedService",
            new[] { typeof(Type), typeof(object) });
    }
}

public static object?[] BindParameters(
    MethodInfo method,
    CommandLineApplication command,
    CancellationToken cancellationToken)
{
    var methodParams = method.GetParameters();
    var arguments = new object?[methodParams.Length];

    for (var i = 0; i < methodParams.Length; i++)
    {
        var methodParam = methodParams[i];

        // Check for keyed services attribute using reflection
        if (TryResolveKeyedService(methodParam, command, out var keyedService))
        {
            arguments[i] = keyedService;
            continue;
        }

        // ... existing parameter binding logic for CancellationToken,
        // CommandLineApplication, etc. ...

        // Fall back to standard service resolution
        var service = command.AdditionalServices?.GetService(methodParam.ParameterType);
        if (service != null)
        {
            arguments[i] = service;
        }
    }

    return arguments;
}

private static bool TryResolveKeyedService(
    ParameterInfo parameter,
    CommandLineApplication command,
    out object? service)
{
    service = null;

    // If keyed services types aren't available, skip
    if (s_fromKeyedServicesAttributeType == null ||
        s_keyProperty == null ||
        s_keyedServiceProviderType == null ||
        s_getKeyedServiceMethod == null)
    {
        return false;
    }

    // Check if parameter has [FromKeyedServices] attribute
    var keyedAttr = parameter.GetCustomAttribute(s_fromKeyedServicesAttributeType);
    if (keyedAttr == null)
    {
        return false;
    }

    // Get the key from the attribute
    var key = s_keyProperty.GetValue(keyedAttr);

    // Check if the service provider supports keyed services
    if (command.AdditionalServices == null ||
        !s_keyedServiceProviderType.IsInstanceOfType(command.AdditionalServices))
    {
        throw new InvalidOperationException(
            $"Parameter '{parameter.Name}' has [FromKeyedServices] attribute, " +
            "but AdditionalServices does not implement IKeyedServiceProvider. " +
            "Ensure you're using a DI container that supports keyed services (.NET 8+).");
    }

    // Invoke GetKeyedService via reflection
    service = s_getKeyedServiceMethod.Invoke(
        command.AdditionalServices,
        new[] { parameter.ParameterType, key });

    if (service == null)
    {
        throw new InvalidOperationException(
            $"No keyed service found for type '{parameter.ParameterType}' " +
            $"with key '{key}'.");
    }

    return true;
}
```

## Benefits of this approach:

1. **No new package dependency** - The core library continues to work without `Microsoft.Extensions.DependencyInjection.Abstractions`

2. **Graceful degradation** - On older frameworks or DI containers, the keyed services feature simply isn't available, but nothing breaks

3. **Performance** - Types and methods are cached in static fields, so reflection cost is only paid once at startup

4. **Consistent pattern** - This follows how the library already handles optional DI integration

## Alternative: Keep it in Hosting.CommandLine

Another option would be to add this feature only to the `McMaster.Extensions.Hosting.CommandLine` package, which already has DI dependencies. Users of the generic host integration would get keyed services support, while the core library stays dependency-free.

Let me know if you'd like me to submit a PR with either approach!
