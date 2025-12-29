// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Adds settings from <see cref="CommandAttribute" /> and <see cref="ValidationAttribute"/> set on the model type for <see cref="CommandLineApplication{TModel}" />.
    /// </summary>
    /// <seealso cref="McMaster.Extensions.CommandLineUtils.Conventions.IConvention" />
    public class CommandAttributeConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            // Use the metadata provider - it uses generated metadata if available,
            // or falls back to reflection-based extraction.
            var provider = context.MetadataProvider;
            if (provider?.CommandInfo != null)
            {
                provider.CommandInfo.ApplyTo(context.Application);
            }
            else
            {
                // Fallback: direct attribute access (for backward compatibility if MetadataProvider is null)
                var attribute = context.ModelType.GetCustomAttribute<CommandAttribute>();
                attribute?.Configure(context.Application);
            }

            foreach (var subcommand in context.Application.Commands)
            {
                if (subcommand is IModelAccessor subcommandAccessor)
                {
                    Apply(new ConventionContext(subcommand, subcommandAccessor.GetModelType()));
                }
            }

            // Note: ValidationAttribute processing still uses reflection as this is
            // about adding validators, not extracting command metadata.
            // The validation attributes are processed by the validation conventions.
            foreach (var attr in context.ModelType.GetCustomAttributes<ValidationAttribute>())
            {
                context.Application.Validators.Add(new AttributeValidator(attr));
            }
        }
    }
}
