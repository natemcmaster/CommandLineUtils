// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Sets a property named <c>Subcommand</c> to the value of the selected subcommand
    /// model type of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class SubcommandPropertyConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var subcommandProp = context.ModelType.GetTypeInfo().GetProperty("Subcommand", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (subcommandProp == null)
            {
                return;
            }

            var setter = ReflectionHelper.GetPropertySetter(subcommandProp);
            context.Application.OnParsed(r =>
            {
                var subCommand = r.SelectedCommand;
                while (subCommand != null)
                {
                    if (ReferenceEquals(subCommand.Parent, context.Application))
                    {
                        if (subCommand is IModelAccessor subcmdAccessor)
                        {
                            setter(context.ModelAccessor.GetModel(), subcmdAccessor.GetModel());
                        }
                        return;
                    }
                    subCommand = subCommand.Parent;
                }
            });
        }
    }
}
