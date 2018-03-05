// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Conventions;
using McMaster.Extensions.CommandLineUtils.HelpText;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Describes a set of command line arguments, options, and execution behavior
    /// using a type of <typeparamref name="TModel" /> to model the application.
    /// </summary>
    public class CommandLineApplication<TModel> : CommandLineApplication, IModelAccessor, IDisposable
        where TModel : class
    {
        private Lazy<TModel> _lazy;

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="CommandLineApplication.ThrowOnUnexpectedArgument"/>.</param>
        public CommandLineApplication(bool throwOnUnexpectedArg = true)
            : base(throwOnUnexpectedArg)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        public CommandLineApplication(IConsole console)
            : base(console)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="CommandLineApplication.ThrowOnUnexpectedArgument"/>.</param>
        public CommandLineApplication(IConsole console, string workingDirectory, bool throwOnUnexpectedArg)
            : base(console, workingDirectory, throwOnUnexpectedArg)
        {
            Initialize();
        }

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
            Initialize();
        }

        internal CommandLineApplication(CommandLineApplication parent, string name, bool throwOnUnexpectedArg)
            : base(parent, name, throwOnUnexpectedArg)
        {
            Initialize();
        }

        private void Initialize()
        {
            _lazy = new Lazy<TModel>(CreateModel);
        }

        /// <summary>
        /// An instance of the model associated with the command line application.
        /// </summary>
        public TModel Model => _lazy.Value;

        object IModelAccessor.GetModel() => Model;

        /// <summary>
        ///  Create an instance of <typeparamref name="TModel" />.
        /// </summary>
        /// <returns>An instance of the context.</returns>
        protected virtual TModel CreateModel()
        {
            return Activator.CreateInstance<TModel>();
        }

        /// <inheritdoc />
        protected override void HandleParseResult(ParseResult parseResult)
        {
            (this as IModelAccessor).GetModel();

            base.HandleParseResult(parseResult);
        }

        private protected override ConventionContext CreateConventionContext() => new ConventionContext(this, typeof(TModel));

        void IDisposable.Dispose()
        {
            if (Model is IDisposable dt)
            {
                dt?.Dispose();
            }

            foreach (var command in Commands)
            {
                if (command is IDisposable dc)
                {
                    dc.Dispose();
                }
            }
        }
    }
}
