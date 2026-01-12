// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// A command line application that uses a model factory instead of reflection.
    /// This is used for AOT-compatible subcommand creation.
    /// </summary>
    internal sealed class CommandLineApplicationWithModel : CommandLineApplication, IModelAccessor
    {
        private readonly Type _modelType;
        private readonly IModelFactory _modelFactory;
        private readonly Lazy<object> _lazyModel;

        internal CommandLineApplicationWithModel(
            CommandLineApplication parent,
            string name,
            Type modelType,
            IModelFactory modelFactory)
            : base(parent, name, modelType)
        {
            _modelType = modelType;
            _modelFactory = modelFactory;
            _lazyModel = new Lazy<object>(() => _modelFactory.Create());
        }

        /// <summary>
        /// Gets the model instance.
        /// </summary>
        public object Model => _lazyModel.Value;

        Type IModelAccessor.GetModelType() => _modelType;

        object IModelAccessor.GetModel() => Model;
    }
}
