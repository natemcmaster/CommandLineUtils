// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Factory for creating model instances without reflection.
    /// </summary>
    public interface IModelFactory
    {
        /// <summary>
        /// Creates a new model instance.
        /// </summary>
        /// <returns>A new instance of the model.</returns>
        object Create();
    }

    /// <summary>
    /// Strongly-typed factory for creating model instances without reflection.
    /// </summary>
    /// <typeparam name="TModel">The model type to create.</typeparam>
    public interface IModelFactory<TModel> : IModelFactory
        where TModel : class
    {
        /// <summary>
        /// Creates a new model instance.
        /// </summary>
        /// <returns>A new instance of the model.</returns>
        new TModel Create();
    }
}
