// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Sets a property named <c>Parent</c> on the model type to the value
    /// of the model of the parent command.
    /// </summary>
    public class ParentPropertyConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            var modelAccessor = context.ModelAccessor;
            if (context.ModelType == null || modelAccessor == null)
            {
                return;
            }

            var parentProp = context.ModelType.GetProperty("Parent", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (parentProp == null)
            {
                return;
            }

            var setter = ReflectionHelper.GetPropertySetter(parentProp);
            context.Application.OnParsingComplete(r =>
            {
                var subcommand = r.SelectedCommand;
                while (subcommand != null)
                {
                    if (ReferenceEquals(context.Application, subcommand))
                    {
                        if (subcommand.Parent is IModelAccessor parentAccessor)
                        {
                            setter(modelAccessor.GetModel(), parentAccessor.GetModel());
                        }
                        return;
                    }
                    subcommand = subcommand.Parent;
                }
            });
        }
    }
}
