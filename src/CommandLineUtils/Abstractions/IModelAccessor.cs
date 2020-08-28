// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// Provides access to a command line application model.
    /// </summary>
    public interface IModelAccessor
    {
        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        /// <returns>The type.</returns>
        Type GetModelType();

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <returns>The model.</returns>
        object GetModel();
    }
}
