// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Errors;

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
            var modelAccessor = context.ModelAccessor;
            if (context.ModelType == null || modelAccessor == null)
            {
                return;
            }

            var attributes = context.ModelType.GetTypeInfo().GetCustomAttributes<SubcommandAttribute>();

            foreach (var attribute in attributes)
            {
                var contextArgs = new object[] { context };
                foreach (var type in attribute.Types)
                {
                    AssertSubcommandIsNotCycled(type, context.Application);

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

        private void AssertSubcommandIsNotCycled(Type modelType, CommandLineApplication? parentCommand)
        {
            while (parentCommand != null)
            {
                if (parentCommand is IModelAccessor parentCommandAccessor
                    && parentCommandAccessor.GetModelType() == modelType)
                {
                    throw new SubcommandCycleException(modelType);
                }
                parentCommand = parentCommand.Parent;
            }
        }

        private static readonly MethodInfo s_addSubcommandMethod
            = typeof(SubcommandAttributeConvention).GetRuntimeMethods()
                .Single(m => m.Name == nameof(AddSubcommandImpl));

        private void AddSubcommandImpl<TSubCommand>(ConventionContext context)
            where TSubCommand : class
        {
            context.Application.Command<TSubCommand>(null, null);
        }
    }
}
