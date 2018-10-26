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
    public class CommandLineApplication<TModel> : CommandLineApplication, IModelAccessor
        where TModel : class
    {
        private Lazy<TModel> _lazy;
        private Func<TModel> _modelFactory = DefaultModelFactory;

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

        private static TModel DefaultModelFactory()
        {
            try
            {
                return Activator.CreateInstance<TModel>();
            }
            catch (MissingMethodException ex)
            {
                throw new MissingParameterlessConstructorException(typeof(TModel), ex);
            }
        }

        /// <summary>
        /// An instance of the model associated with the command line application.
        /// </summary>
        public TModel Model => _lazy.Value;

        Type IModelAccessor.GetModelType() => typeof(TModel);

        object IModelAccessor.GetModel() => Model;

        /// <summary>
        ///  Create an instance of <typeparamref name="TModel" />.
        /// </summary>
        /// <returns>An instance of the context.</returns>
        protected virtual TModel CreateModel() => ModelFactory();

        /// <summary>
        /// Defines the function that produces an instance of <typeparamref name="TModel" />.
        /// </summary>
        public Func<TModel> ModelFactory
        {
            get => _modelFactory;
            set => _modelFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        protected override void HandleParseResult(ParseResult parseResult)
        {
            (this as IModelAccessor).GetModel();

            base.HandleParseResult(parseResult);
        }

        private protected override ConventionContext CreateConventionContext() => new ConventionContext(this, typeof(TModel));

        /// <inheritdoc />
        public override void Dispose()
        {
            if (Model is IDisposable dt)
            {
                dt.Dispose();
            }

            base.Dispose();
        }
    }
}
