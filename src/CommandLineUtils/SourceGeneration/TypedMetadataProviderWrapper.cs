// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Wrapper to provide typed access to an untyped metadata provider.
    /// </summary>
    internal sealed class TypedMetadataProviderWrapper<TModel> : ICommandMetadataProvider<TModel>
        where TModel : class
    {
        private readonly ICommandMetadataProvider _inner;

        public TypedMetadataProviderWrapper(ICommandMetadataProvider inner)
        {
            _inner = inner;
        }

        public Type ModelType => _inner.ModelType;
        public IReadOnlyList<OptionMetadata> Options => _inner.Options;
        public IReadOnlyList<ArgumentMetadata> Arguments => _inner.Arguments;
        public IReadOnlyList<SubcommandMetadata> Subcommands => _inner.Subcommands;
        public CommandMetadata? CommandInfo => _inner.CommandInfo;
        public IExecuteHandler? ExecuteHandler => _inner.ExecuteHandler;
        public IValidateHandler? ValidateHandler => _inner.ValidateHandler;
        public IValidationErrorHandler? ValidationErrorHandler => _inner.ValidationErrorHandler;
        public SpecialPropertiesMetadata? SpecialProperties => _inner.SpecialProperties;
        public HelpOptionMetadata? HelpOption => _inner.HelpOption;
        public VersionOptionMetadata? VersionOption => _inner.VersionOption;

        IModelFactory ICommandMetadataProvider.GetModelFactory(IServiceProvider? services) => _inner.GetModelFactory(services);

        public IModelFactory<TModel> GetModelFactory(IServiceProvider? services)
        {
            var factory = _inner.GetModelFactory(services);
            if (factory is IModelFactory<TModel> typedFactory)
            {
                return typedFactory;
            }
            return new TypedModelFactoryWrapper<TModel>(factory);
        }
    }
}
