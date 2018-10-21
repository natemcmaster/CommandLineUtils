using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// The exception that is thrown when trying to intianciate a model with no parameterless constructor.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to instanciate</typeparam>
    public class MissingParameterlessConstructorException<TModel> : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="MissingParameterlessConstructorException{TModel}"/>.
        /// </summary>
        /// <param name="innerException">The original exception.</param>
        public MissingParameterlessConstructorException(Exception innerException) : base($"Class {typeof(TModel).FullName} does not have a parameterless constructor", innerException) { }
    }
}
