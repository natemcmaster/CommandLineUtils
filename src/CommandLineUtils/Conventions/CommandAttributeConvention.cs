// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Adds settings from <see cref="CommandAttribute"/> set
    /// on the model type for <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class CommandAttributeConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var attribute = context.ModelType.GetTypeInfo().GetCustomAttribute<CommandAttribute>();
            attribute?.Configure(context.Application);
        }
    }
}
