// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Provides metadata about a command model type.
    /// Implemented by both reflection-based and source-generated providers.
    /// </summary>
    public interface ICommandMetadataProvider
    {
        /// <summary>
        /// Gets the model type this provider handles.
        /// </summary>
        Type ModelType { get; }

        /// <summary>
        /// Gets metadata for all options defined on the model.
        /// </summary>
        IReadOnlyList<OptionMetadata> Options { get; }

        /// <summary>
        /// Gets metadata for all arguments defined on the model.
        /// </summary>
        IReadOnlyList<ArgumentMetadata> Arguments { get; }

        /// <summary>
        /// Gets metadata for subcommand types.
        /// </summary>
        IReadOnlyList<SubcommandMetadata> Subcommands { get; }

        /// <summary>
        /// Gets metadata about the command itself (name, description, etc.).
        /// </summary>
        CommandMetadata? CommandInfo { get; }

        /// <summary>
        /// Gets the execute handler if one is defined (OnExecute/OnExecuteAsync).
        /// </summary>
        IExecuteHandler? ExecuteHandler { get; }

        /// <summary>
        /// Gets the validate handler if one is defined (OnValidate).
        /// </summary>
        IValidateHandler? ValidateHandler { get; }

        /// <summary>
        /// Gets the validation error handler if one is defined (OnValidationError).
        /// </summary>
        IValidationErrorHandler? ValidationErrorHandler { get; }

        /// <summary>
        /// Creates a model factory for the type.
        /// </summary>
        /// <param name="services">Optional service provider for dependency injection.</param>
        /// <returns>A factory that can create model instances.</returns>
        IModelFactory GetModelFactory(IServiceProvider? services);

        /// <summary>
        /// Gets property accessors for setting Parent, RemainingArgs, Subcommand.
        /// </summary>
        SpecialPropertiesMetadata? SpecialProperties { get; }

        /// <summary>
        /// Gets help option metadata if defined.
        /// </summary>
        HelpOptionMetadata? HelpOption { get; }

        /// <summary>
        /// Gets version option metadata if defined.
        /// </summary>
        VersionOptionMetadata? VersionOption { get; }
    }

    /// <summary>
    /// Strongly-typed metadata provider for a specific model type.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface ICommandMetadataProvider<TModel> : ICommandMetadataProvider
        where TModel : class
    {
        /// <summary>
        /// Creates a model factory for the type.
        /// </summary>
        /// <param name="services">Optional service provider for dependency injection.</param>
        /// <returns>A factory that can create model instances.</returns>
        new IModelFactory<TModel> GetModelFactory(IServiceProvider? services);
    }
}
