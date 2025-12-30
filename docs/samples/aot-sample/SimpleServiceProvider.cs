// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace AotSample
{

    /// <summary>
    /// Simple IServiceProvider implementation for DI testing.
    /// </summary>
    public class SimpleServiceProvider : IServiceProvider
    {

        private readonly Dictionary<Type, object> _services = new();

        public void Register<T>(T instance) where T : notnull
        {
            _services[typeof(T)] = instance;
        }

        public object? GetService(Type serviceType)
        {
            return _services.TryGetValue(serviceType, out var service) ? service : null;
        }

    }

}
