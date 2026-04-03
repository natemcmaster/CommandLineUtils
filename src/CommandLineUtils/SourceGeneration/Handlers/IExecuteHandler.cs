// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Handles execution of the OnExecute/OnExecuteAsync method without reflection.
    /// </summary>
    public interface IExecuteHandler
    {
        /// <summary>
        /// Whether this is an async handler (OnExecuteAsync vs OnExecute).
        /// </summary>
        bool IsAsync { get; }

        /// <summary>
        /// Invokes the execute method on the model.
        /// </summary>
        /// <param name="model">The model instance.</param>
        /// <param name="app">The command line application.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The exit code.</returns>
        Task<int> InvokeAsync(object model, CommandLineApplication app, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Strongly-typed handler for executing the OnExecute/OnExecuteAsync method.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IExecuteHandler<TModel> : IExecuteHandler
        where TModel : class
    {
        /// <summary>
        /// Invokes the execute method on the model.
        /// </summary>
        /// <param name="model">The model instance.</param>
        /// <param name="app">The command line application.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The exit code.</returns>
        Task<int> InvokeAsync(TModel model, CommandLineApplication app, CancellationToken cancellationToken);
    }
}
