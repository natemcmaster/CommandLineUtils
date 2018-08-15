// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Creates a subcommand for each <see cref="McMaster.Extensions.CommandLineUtils.SubcommandAttribute"/>
    /// on the model type of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class SubcommandAttributeConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var attributes = context.ModelType.GetTypeInfo().GetCustomAttributes<SubcommandAttribute>();

            foreach (var attribute in attributes)
            {
                var contextArgs = new object[] { context, attribute };
                foreach (var type in attribute.Types)
                {
                    var impl = s_addSubcommandMethod.MakeGenericMethod(type);
                    try
                    {
                        impl.Invoke(this, contextArgs);
                    }
                    catch (TargetInvocationException ex)
                    {
                        // unwrap
                        throw ex.InnerException ?? ex;
                    }
                }
            }
        }

        private static readonly MethodInfo s_addSubcommandMethod
            = typeof(SubcommandAttributeConvention).GetRuntimeMethods()
                .Single(m => m.Name == nameof(AddSubcommandImpl));

        private void AddSubcommandImpl<TSubCommand>(ConventionContext context, SubcommandAttribute subcommand)
            where TSubCommand : class
        {
#pragma warning disable 618
            context.Application.Command<TSubCommand>(subcommand.Name, subcommand.Configure);
#pragma warning restore 618
        }
    }
}
