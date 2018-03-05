// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Sets <see cref="CommandLineApplication.OptionVersion"/> using settings from
    /// <see cref="VersionOptionFromMemberAttribute"/> on the model type of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class VersionOptionFromMemberAttributeConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var versionOptionFromMember = context.ModelType.GetTypeInfo().GetCustomAttribute<VersionOptionFromMemberAttribute>();
            versionOptionFromMember?.Configure(context.Application, context.ModelType, context.ModelAccessor.GetModel);
        }
    }
}
