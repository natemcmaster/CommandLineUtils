// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// The context in which a convention is applied.
    /// </summary>
    public class ConventionContext
    {
        private ICommandMetadataProvider? _metadataProvider;

        /// <summary>
        /// Initializes an instance of <see cref="ConventionContext" />.
        /// </summary>
        /// <param name="application">The application</param>
        /// <param name="modelType">The type of the model.</param>
        public ConventionContext(CommandLineApplication application, Type? modelType)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            ModelType = modelType;
        }

        /// <summary>
        /// The application to which the convention is applied.
        /// </summary>
        public CommandLineApplication Application { get; private set; }

        /// <summary>
        /// The type of the application model. Can be null when applied to <see cref="CommandLineApplication" />
        /// instead of <see cref="CommandLineApplication{TModel}" />.
        /// </summary>
        public Type? ModelType { get; private set; }

        /// <summary>
        /// A convenience accessor for getting the application model object.
        /// Can be null when applied to <see cref="CommandLineApplication" /> instead of
        /// <see cref="CommandLineApplication{TModel}" />.
        /// </summary>
        public IModelAccessor? ModelAccessor => Application as IModelAccessor;

        /// <summary>
        /// Gets the metadata provider for the model type.
        /// Returns null if <see cref="ModelType"/> is null.
        /// This provider may contain source-generated metadata (AOT-friendly)
        /// or fall back to reflection-based metadata extraction.
        /// </summary>
        public ICommandMetadataProvider? MetadataProvider
        {
            get
            {
                if (_metadataProvider == null && ModelType != null)
                {
                    _metadataProvider = DefaultMetadataResolver.Instance.GetProvider(ModelType);
                }
                return _metadataProvider;
            }
        }

        /// <summary>
        /// Gets a value indicating whether source-generated metadata is available for the model type.
        /// When true, metadata was generated at compile time and can be used without reflection.
        /// </summary>
        public bool HasGeneratedMetadata
        {
            get
            {
                if (ModelType == null)
                {
                    return false;
                }
                return DefaultMetadataResolver.Instance.HasGeneratedMetadata(ModelType);
            }
        }
    }
}
