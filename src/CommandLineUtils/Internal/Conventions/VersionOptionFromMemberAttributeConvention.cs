// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class VersionOptionFromMemberAttributeConvention : IConvention
    {
        public void Apply(ConventionContext context)
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
