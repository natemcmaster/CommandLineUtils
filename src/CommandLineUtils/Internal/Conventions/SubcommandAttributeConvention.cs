// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class SubcommandAttributeConvention : IConvention
    {
        public void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var subcommands = context.ModelType.GetTypeInfo().GetCustomAttributes<SubcommandAttribute>();
            if (subcommands == null)
            {
                return;
            }

            foreach (var subcommand in subcommands)
            {
                var impl = s_addSubcommandMethod.MakeGenericMethod(subcommand.CommandType);
                try
                {
                    impl.Invoke(this, new object[] { context, subcommand });
                }
                catch (TargetInvocationException ex)
                {
                    // unwrap
                    throw ex.InnerException ?? ex;
                }
            }
        }

        private static readonly MethodInfo s_addSubcommandMethod
            = typeof(SubcommandAttributeConvention).GetRuntimeMethods().Single(m => m.Name == nameof(AddSubcommandImpl));

        private void AddSubcommandImpl<TSubCommand>(ConventionContext context, SubcommandAttribute subcommand)
            where TSubCommand : class, new()
        {
            if (context.Application.Commands.Any(c => c.Name.Equals(subcommand.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(Strings.DuplicateSubcommandName(subcommand.Name));
            }

            context.Application.Command<TSubCommand>(subcommand.Name, subcommand.Configure);
        }
    }
}
