// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Model factory that uses Activator.CreateInstance or DI.
    /// </summary>
    [RequiresUnreferencedCode("Uses Activator.CreateInstance or DI")]
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
                var instance = _services.GetService(_modelType);
                if (instance != null)
                {
                    return instance;
                }
            }

            // Fall back to Activator
            return Activator.CreateInstance(_modelType)
                ?? throw new InvalidOperationException($"Failed to create instance of {_modelType.FullName}");
        }
    }
}
