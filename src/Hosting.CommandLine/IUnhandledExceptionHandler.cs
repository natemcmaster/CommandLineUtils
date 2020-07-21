using System;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.Hosting.CommandLine.Internal;

namespace McMaster.Extensions.Hosting.CommandLine
{
    /// <summary>
    /// Used by <see cref="CommandLineLifetime"/> to handle exceptions that are emitted from the
    /// <see cref="CommandLineApplication{TModel}"/> e.g. during parsing or execution
    /// </summary>
    public interface IUnhandledExceptionHandler
    {
        /// <summary>
        /// Handle otherwise uncaught exception. You are free to log, rethrow, … the exception 
        /// </summary>
        /// <param name="e">An otherwise uncaught exception</param>
        void HandleException(Exception e);
    }
}
