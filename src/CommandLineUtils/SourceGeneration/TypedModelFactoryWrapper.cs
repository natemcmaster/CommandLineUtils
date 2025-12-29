// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Wrapper to provide typed access to an untyped model factory.
    /// </summary>
    internal sealed class TypedModelFactoryWrapper<TModel> : IModelFactory<TModel>
        where TModel : class
    {
        private readonly IModelFactory _inner;

        public TypedModelFactoryWrapper(IModelFactory inner)
        {
            _inner = inner;
        }

        public TModel Create() => (TModel)_inner.Create();
        object IModelFactory.Create() => _inner.Create();
    }
}
