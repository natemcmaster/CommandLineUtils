// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Default implementation that checks the registry first, falls back to reflection.
    /// </summary>
    public sealed class DefaultMetadataResolver : IMetadataResolver
    {
        /// <summary>
        /// Gets the singleton instance of the default metadata resolver.
        /// </summary>
        public static readonly DefaultMetadataResolver Instance = new();

        private readonly ConcurrentDictionary<Type, ICommandMetadataProvider> _reflectionProviders = new();

        private DefaultMetadataResolver()
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method first checks for source-generated metadata (AOT-friendly).
        /// If no generated metadata exists, it falls back to reflection-based metadata extraction.
        /// For full AOT compatibility, ensure the CommandLineUtils.Generators package is referenced
        /// and the source generator runs during compilation.
        /// </remarks>
#if NET6_0_OR_GREATER
        [RequiresUnreferencedCode("Falls back to reflection when no generated metadata is available. Use the source generator for AOT compatibility.")]
#endif
        public ICommandMetadataProvider GetProvider(Type modelType)
        {
            // Check for generated metadata first (AOT-safe path)
            if (CommandMetadataRegistry.TryGetProvider(modelType, out var provider))
            {
                return provider;
            }

            // Fall back to reflection-based provider (requires reflection)
            return _reflectionProviders.GetOrAdd(modelType, CreateReflectionProvider);
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method first checks for source-generated metadata (AOT-friendly).
        /// If no generated metadata exists, it falls back to reflection-based metadata extraction.
        /// For full AOT compatibility, ensure the CommandLineUtils.Generators package is referenced
        /// and the source generator runs during compilation.
        /// </remarks>
#if NET6_0_OR_GREATER
        [RequiresUnreferencedCode("Falls back to reflection when no generated metadata is available. Use the source generator for AOT compatibility.")]
#endif
        public ICommandMetadataProvider<TModel> GetProvider<TModel>() where TModel : class
        {
            // Check for generated metadata first (AOT-safe path)
            if (CommandMetadataRegistry.TryGetProvider<TModel>(out var provider))
            {
                return provider;
            }

            // Fall back to reflection-based provider (requires reflection)
            var untypedProvider = _reflectionProviders.GetOrAdd(typeof(TModel), CreateReflectionProvider);

            // The reflection provider should implement the generic interface
            if (untypedProvider is ICommandMetadataProvider<TModel> typedProvider)
            {
                return typedProvider;
            }

            // Wrap it if needed
            return new TypedMetadataProviderWrapper<TModel>(untypedProvider);
        }

        /// <inheritdoc />
        public bool HasGeneratedMetadata(Type modelType)
        {
            return CommandMetadataRegistry.HasMetadata(modelType);
        }

#if NET6_0_OR_GREATER
        [RequiresUnreferencedCode("Uses reflection to analyze the model type")]
#endif
        private static ICommandMetadataProvider CreateReflectionProvider(Type modelType)
        {
            // This creates a reflection-based implementation of ICommandMetadataProvider
            // Will be implemented in Phase 2
            return new ReflectionMetadataProvider(modelType);
        }

        /// <summary>
        /// Clears all cached reflection providers. Primarily for testing.
        /// </summary>
        internal void ClearCache()
        {
            _reflectionProviders.Clear();
        }
    }
}
