// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Resolves command metadata for a type using either generated or reflection-based providers.
    /// </summary>
    public interface IMetadataResolver
    {
        /// <summary>
        /// Gets the metadata provider for a type.
        /// Returns generated metadata if available, otherwise uses reflection.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <returns>A metadata provider for the type.</returns>
        ICommandMetadataProvider GetProvider(Type modelType);

        /// <summary>
        /// Gets the metadata provider for a type.
        /// Returns generated metadata if available, otherwise uses reflection.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <returns>A metadata provider for the type.</returns>
        ICommandMetadataProvider<TModel> GetProvider<TModel>() where TModel : class;

        /// <summary>
        /// Checks if generated metadata is available for a type.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <returns>True if generated metadata is available; otherwise, false.</returns>
        bool HasGeneratedMetadata(Type modelType);
    }
}
