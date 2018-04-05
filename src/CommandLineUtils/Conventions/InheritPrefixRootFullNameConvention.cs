// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Adds settings from <see cref="PrefixRootFullNameAttribute"/> set
    /// on the model type for <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class InheritPrefixRootFullNameConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            var setByAttr = false;
            if (context.ModelType != null)
            {
                var attribute = context.ModelType.GetTypeInfo().GetCustomAttribute<PrefixRootFullNameAttribute>();
                if (attribute != null)
                {
                    attribute.Configure(context.Application);
                    setByAttr = true;
                }
            }

            if (!setByAttr)
            {
                context.Application.DeterminePrefixRootFullNameByInheritance();
            }
            
        }
    }
}
