// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
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

#nullable disable
        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication{TModel}"/>.
        /// </summary>
        public CommandLineApplication()
            : base()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication{TModel}"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        public CommandLineApplication(IConsole console)
            : base(console)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication{TModel}"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        public CommandLineApplication(IConsole console, string workingDirectory)
            : base(console, workingDirectory)
        {
            Initialize();
        }

        /// <summary>
        /// <para>
        /// This constructor is obsolete and will be removed in a future version.
        /// The recommended replacement is <see cref="CommandLineApplication{TModel}(IHelpTextGenerator, IConsole, string)" />
        /// </para>
        /// <para>
        /// Initializes a new instance of <see cref="CommandLineApplication{TModel}"/>.
        /// </para>
        /// </summary>
        /// <param name="helpTextGenerator">The help text generator to use.</param>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        public CommandLineApplication(IHelpTextGenerator helpTextGenerator, IConsole console, string workingDirectory)
            : base(helpTextGenerator, console, workingDirectory)
        {
            Initialize();
        }

        /// <summary>
        /// <para>
        /// This constructor is obsolete and will be removed in a future version.
        /// The recommended replacement is <see cref="CommandLineApplication{TModel}()" />.
        /// See https://github.com/natemcmaster/CommandLineUtils/issues/339 for details.
        /// </para>
        /// <para>
        /// Initializes a new instance of <see cref="CommandLineApplication{TModel}"/>.
        /// </para>
        /// </summary>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="CommandLineApplication.ThrowOnUnexpectedArgument"/>.</param>
        [Obsolete("This constructor is obsolete and will be removed in a future version. " +
            "The recommended replacement is CommandLineApplication<T>() and UnrecognizedArgumentHandling. " +
            "See https://github.com/natemcmaster/CommandLineUtils/issues/339 for details.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CommandLineApplication(bool throwOnUnexpectedArg)
            : base(throwOnUnexpectedArg)
        {
            Initialize();
        }

        /// <summary>
        /// <para>
        /// This constructor is obsolete and will be removed in a future version.
        /// The recommended replacement is <see cref="CommandLineApplication{TModel}(IConsole, string)" />
        /// See https://github.com/natemcmaster/CommandLineUtils/issues/339 for details.
        /// </para>
        /// <para>
        /// Initializes a new instance of <see cref="CommandLineApplication{TModel}"/>.
        /// </para>
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="CommandLineApplication.ThrowOnUnexpectedArgument"/>.</param>
        [Obsolete("This constructor is obsolete and will be removed in a future version. " +
            "The recommended replacement is CommandLineApplication<T>(IConsole, string) and UnrecognizedArgumentHandling. " +
            "See https://github.com/natemcmaster/CommandLineUtils/issues/339 for details.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CommandLineApplication(IConsole console, string workingDirectory, bool throwOnUnexpectedArg)
            : base(console, workingDirectory, throwOnUnexpectedArg)
        {
            Initialize();
        }

        /// <summary>
        /// <para>
        /// This constructor is obsolete and will be removed in a future version.
        /// The recommended replacement is <see cref="CommandLineApplication{TModel}(IHelpTextGenerator, IConsole, string)" />
        /// See https://github.com/natemcmaster/CommandLineUtils/issues/339 for details.
        /// </para>
        /// <para>
        /// Initializes a new instance of <see cref="CommandLineApplication{TModel}"/>.
        /// </para>
        /// </summary>
        /// <param name="helpTextGenerator">The help text generator to use.</param>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="CommandLineApplication.ThrowOnUnexpectedArgument"/>.</param>
        [Obsolete("This constructor is obsolete and will be removed in a future version. " +
            "The recommended replacement is CommandLineApplication<T>(IHelpTextGenerator, IConsole, string) and UnrecognizedArgumentHandling. " +
            "See https://github.com/natemcmaster/CommandLineUtils/issues/339 for details.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CommandLineApplication(IHelpTextGenerator helpTextGenerator, IConsole console, string workingDirectory, bool throwOnUnexpectedArg)
            : base(helpTextGenerator, console, workingDirectory, throwOnUnexpectedArg)
        {
            Initialize();
        }

        internal CommandLineApplication(CommandLineApplication parent, string name)
            : base(parent, name)
        {
            Initialize();
        }
#nullable enable

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
        // TODO remove in 3.0. This doesn't do anything anymore.
        protected override void HandleParseResult(ParseResult parseResult)
        {
            base.HandleParseResult(parseResult);
        }

        private protected override ConventionContext CreateConventionContext() => new ConventionContext(this, typeof(TModel));

        /// <inheritdoc />
        public override void Dispose()
        {
            if (_lazy.IsValueCreated && Model is IDisposable dt)
            {
                dt.Dispose();
            }

            base.Dispose();
        }
    }
}
