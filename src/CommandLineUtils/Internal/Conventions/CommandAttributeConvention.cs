// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class CommandAttributeConvention : IConvention
    {
        public void Apply(ConventionContext context)
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
