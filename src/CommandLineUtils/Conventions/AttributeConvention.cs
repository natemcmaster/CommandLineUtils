// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Searches the model type and its members for attributes that implement <see cref="IMemberConvention"/> or <see cref="IConvention"/>.
    /// </summary>
    public class AttributeConvention : IConvention
    {
        /// <inheritdoc />
        public void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            foreach (var attr in context.ModelType.GetCustomAttributes().OfType<IConvention>())
            {
                attr.Apply(context);
            }

            var members = ReflectionHelper.GetMembers(context.ModelType);
            foreach (var member in members)
            {
                foreach (var attr in member.GetCustomAttributes().OfType<IMemberConvention>())
                {
                    attr.Apply(context, member);
                }
            }
        }
    }
}
