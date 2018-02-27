// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.HelpText;
using McMaster.Extensions.CommandLineUtils.Internal;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Describes a set of command line arguments, options, and execution behavior
    /// using a type of <typeparamref name="TModel" /> to model the application.
    /// </summary>
    public class CommandLineApplication<TModel> : CommandLineApplication
        where TModel : class
    {
        private List<Action<TModel>> _onInitialize;

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="CommandLineApplication.ThrowOnUnexpectedArgument"/>.</param>
        public CommandLineApplication(bool throwOnUnexpectedArg = true)
            : base(throwOnUnexpectedArg)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        public CommandLineApplication(IConsole console)
            : base(console)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="CommandLineApplication.ThrowOnUnexpectedArgument"/>.</param>
        public CommandLineApplication(IConsole console, string workingDirectory, bool throwOnUnexpectedArg)
            : base(console, workingDirectory, throwOnUnexpectedArg)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="helpTextGenerator">The help text generator to use.</param>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="CommandLineApplication.ThrowOnUnexpectedArgument"/>.</param>
        public CommandLineApplication(IHelpTextGenerator helpTextGenerator, IConsole console, string workingDirectory, bool throwOnUnexpectedArg)
            : base(helpTextGenerator, console, workingDirectory, throwOnUnexpectedArg)
        {
        }

        internal CommandLineApplication(CommandLineApplication parent, string name, bool throwOnUnexpectedArg)
            : base(parent, name, throwOnUnexpectedArg)
        {
        }

        // TODO: make experimental API public after it settles.

        /// <summary>
        /// /// Create an instance of <typeparamref name="TModel" />.
        /// </summary>
        /// <returns>An instance of the context.</returns>
        internal virtual TModel CreateModel()
        {
            return Activator.CreateInstance<TModel>();
        }

        /// <summary>
        /// An instance of the model associated with the command line application.
        /// </summary>
        internal TModel Model { get; private set; }

        /// <summary>
        /// Adds a callback that will be invoked when an instance of <typeparamref name="TModel" /> is created.
        /// </summary>
        /// <param name="action"></param>
        internal void OnInitalize(Action<TModel> action)
        {
            _onInitialize = _onInitialize ?? new List<Action<TModel>>();
            _onInitialize.Add(action);
        }

        /// <summary>
        /// Create an instance of <typeparamref name="TModel" />.
        /// </summary>
        /// <returns>An instance of the app.</returns>
        internal void Initialize()
        {
            Model = CreateModel();
            if (_onInitialize != null)
            {
                foreach (var action in _onInitialize)
                {
                    action?.Invoke(Model);
                }
            }
        }
    }
}
