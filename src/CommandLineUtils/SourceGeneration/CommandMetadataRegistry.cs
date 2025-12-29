// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Registry for source-generated command metadata.
    /// Source generators register metadata providers here via module initializers.
    /// </summary>
    public static class CommandMetadataRegistry
    {
        private static readonly ConcurrentDictionary<Type, ICommandMetadataProvider> s_providers = new();

        /// <summary>
        /// Registers a metadata provider for a type.
        /// Called by generated code in module initializers.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="provider">The metadata provider.</param>
        public static void Register<TModel>(ICommandMetadataProvider<TModel> provider)
            where TModel : class
        {
            s_providers[typeof(TModel)] = provider;
        }

        /// <summary>
        /// Registers a metadata provider for a type.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <param name="provider">The metadata provider.</param>
        public static void Register(Type modelType, ICommandMetadataProvider provider)
        {
            s_providers[modelType] = provider;
        }

        /// <summary>
        /// Tries to get the metadata provider for a type.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <param name="provider">The metadata provider, if found.</param>
        /// <returns>True if a provider was found; otherwise, false.</returns>
        public static bool TryGetProvider(Type modelType, [NotNullWhen(true)] out ICommandMetadataProvider? provider)
        {
            return s_providers.TryGetValue(modelType, out provider);
        }

        /// <summary>
        /// Tries to get the metadata provider for a type.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="provider">The metadata provider, if found.</param>
        /// <returns>True if a provider was found; otherwise, false.</returns>
        public static bool TryGetProvider<TModel>([NotNullWhen(true)] out ICommandMetadataProvider<TModel>? provider)
            where TModel : class
        {
            if (s_providers.TryGetValue(typeof(TModel), out var untypedProvider) &&
                untypedProvider is ICommandMetadataProvider<TModel> typedProvider)
            {
                provider = typedProvider;
                return true;
            }

            provider = null;
            return false;
        }

        /// <summary>
        /// Checks if metadata is available for a type.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <returns>True if generated metadata is available; otherwise, false.</returns>
        public static bool HasMetadata(Type modelType)
        {
            return s_providers.ContainsKey(modelType);
        }

        /// <summary>
        /// Checks if metadata is available for a type.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <returns>True if generated metadata is available; otherwise, false.</returns>
        public static bool HasMetadata<TModel>()
            where TModel : class
        {
            return s_providers.ContainsKey(typeof(TModel));
        }

        /// <summary>
        /// Clears all registered providers. Primarily for testing.
        /// </summary>
        internal static void Clear()
        {
            s_providers.Clear();
        }
    }
}
