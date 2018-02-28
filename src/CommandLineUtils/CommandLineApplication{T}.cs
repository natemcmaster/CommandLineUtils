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
    public class CommandLineApplication<TModel> : CommandLineApplication, IConventionBuilder, IModelAccessor
        where TModel : class
    {
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

        /// <summary>
        /// An instance of the model associated with the command line application.
        /// </summary>
        public TModel Model { get; private set; }

        object IModelAccessor.GetModel() => Model;

        /// <summary>
        /// Gets a builder that can be used to apply conventions to
        /// </summary>
        public IConventionBuilder Conventions => this;

        IConventionBuilder IConventionBuilder.AddConvention(IConvention convention)
        {
            var context = new ConventionContext(this, typeof(TModel));
            convention.Apply(context);
            return Conventions;
        }

        /// <summary>
        ///  Create an instance of <typeparamref name="TModel" />.
        /// </summary>
        /// <returns>An instance of the context.</returns>
        protected virtual TModel CreateModel()
        {
            return Activator.CreateInstance<TModel>();
        }

        // TODO: make experimental API public after it settles.
        internal void Initialize()
        {
            Model = CreateModel();
        }
    }
}
