// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Errors
{
    /// <summary>
    /// The exception that is thrown when a subcommand cycle is detected
    /// </summary>
    public class SubcommandCycleException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="SubcommandCycleException"/>.
        /// </summary>
        /// <param name="modelType">The type of the cycled command model</param>
        public SubcommandCycleException(Type modelType)
            : base($"Subcommand cycle detected: trying to add command of model {modelType} as its own direct or indirect subcommand")
        {
            ModelType = modelType;
        }

        /// <summary>
        /// The type of the cycled command model
        /// </summary>
        public Type ModelType { get; }
    }
}
