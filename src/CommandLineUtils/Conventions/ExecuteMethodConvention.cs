// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Sets a command handler to call a method named
    /// <c>OnExecute</c> or <c>OnExecuteAsync</c> on the model type
    /// of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class ExecuteMethodConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            var modelAccessor = context.ModelAccessor;
            if (modelAccessor == null)
            {
                return; // No model, nothing to do
            }

            // MetadataProvider is always available (generated or reflection-based via DefaultMetadataResolver)
            var provider = context.MetadataProvider;
            var handler = provider?.ExecuteHandler;

            if (handler == null)
            {
                // If no OnExecute method exists, set a handler that throws when invoked.
                // This allows commands with subcommands to work (the subcommand will be selected instead),
                // but will throw if the main command is executed directly.
                context.Application.OnExecuteAsync(_ =>
                    throw new InvalidOperationException(Strings.NoOnExecuteMethodFound));
                return;
            }

            // Use the execute handler from metadata (works for both generated and reflection providers)
            context.Application.OnExecuteAsync(async ct =>
            {
                var model = modelAccessor.GetModel();
                return await handler.InvokeAsync(model, context.Application, ct);
            });
        }
    }
}
