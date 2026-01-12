// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.Conventions;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Invokes a method named "OnValidate" on the model type after parsing.
    /// </summary>
    public class ValidateMethodConvention : IConvention
    {
        /// <inheritdoc />
        public void Apply(ConventionContext context)
        {
            var modelAccessor = context.ModelAccessor;
            if (modelAccessor == null)
            {
                return;
            }

            // MetadataProvider is always available (generated or reflection-based via DefaultMetadataResolver)
            var provider = context.MetadataProvider;
            if (provider?.ValidateHandler == null)
            {
                return;
            }

            context.Application.OnValidate(ctx =>
            {
                return provider.ValidateHandler.Invoke(modelAccessor.GetModel(), ctx, context.Application._context)
                    ?? ValidationResult.Success!;
            });
        }
    }
}
