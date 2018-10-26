using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// The exception that is thrown when trying to instantiate a model with no parameterless constructor.
    /// </summary>
    public class MissingParameterlessConstructorException : Exception
    {
        /// <summary>
        /// Gets the type that caused the exception.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="MissingParameterlessConstructorException" />.
        /// </summary>
        /// <param name="type">The type missing a parameterless constructor.</param>
        /// <param name="innerException">The original exception.</param>
        public MissingParameterlessConstructorException(Type type, Exception innerException) : base($"Class {type.FullName} does not have a parameterless constructor", innerException)
        {
            Type = type;
        }
    }
}
