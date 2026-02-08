// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Model factory that uses Activator.CreateInstance or DI with constructor injection.
    /// </summary>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Uses Activator.CreateInstance or DI with constructor injection")]
#elif NET472_OR_GREATER
#else
#error Target framework misconfiguration
#endif
    internal sealed class ActivatorModelFactory : IModelFactory
    {
        private readonly Type _modelType;
        private readonly IServiceProvider? _services;

        public ActivatorModelFactory(Type modelType, IServiceProvider? services = null)
        {
            _modelType = modelType;
            _services = services;
        }

        public object Create()
        {
            // Try DI first if services are available
            if (_services != null)
            {
                // Try to get the model directly from services
                var instance = _services.GetService(_modelType);
                if (instance != null)
                {
                    return instance;
                }

                // Try constructor injection
                instance = TryCreateWithConstructorInjection();
                if (instance != null)
                {
                    return instance;
                }
            }

            // Fall back to Activator (parameterless constructor)
            return Activator.CreateInstance(_modelType)
                ?? throw new InvalidOperationException($"Failed to create instance of {_modelType.FullName}");
        }

        private object? TryCreateWithConstructorInjection()
        {
            // Get all public constructors, ordered by parameter count (descending)
            var constructors = _modelType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(c => c.GetParameters().Length)
                .ToList();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length == 0)
                {
                    // Skip parameterless constructor, handled by Activator.CreateInstance
                    continue;
                }

                var args = new object?[parameters.Length];
                var allResolved = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    var service = _services!.GetService(paramType);

                    if (service == null && !parameters[i].HasDefaultValue)
                    {
                        allResolved = false;
                        break;
                    }

                    args[i] = service ?? parameters[i].DefaultValue;
                }

                if (allResolved)
                {
                    return constructor.Invoke(args);
                }
            }

            return null;
        }
    }
}
