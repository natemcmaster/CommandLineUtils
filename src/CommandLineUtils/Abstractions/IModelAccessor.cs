// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    /// <summary>
    /// Provides access to a command line application model.
    /// </summary>
    public interface IModelAccessor
    {
        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <returns>The model.</returns>
        object GetModel();
    }
}
