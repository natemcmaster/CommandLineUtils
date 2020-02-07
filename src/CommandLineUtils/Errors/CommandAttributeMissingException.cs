using System;
using System.Collections.Generic;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils.Errors
{
    /// <summary>
    /// The exception that is thrown when a subcommand does not have a Command attribute.
    /// </summary>
    public class CommandAttributeMissingException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="SubcommandCycleException"/>.
        /// </summary>
        /// <param name="modelType">The type of the cycled command model</param>
        public CommandAttributeMissingException(Type modelType)
            : base($"A required Command attribute was not found on indicated subcommand type {modelType.Name}.")
        {
            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
        }

        /// <summary>
        /// The type of the cycled command model
        /// </summary>
        public Type ModelType { get; }
    }
}
