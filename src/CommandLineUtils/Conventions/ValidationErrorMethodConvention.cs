// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Invokes a method named <c>OnValidationError</c> on the model type of <see cref="CommandLineApplication{TModel}"/>
    /// to handle validation errors.
    /// </summary>
    public class ValidationErrorMethodConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var method = context.ModelType
                .GetTypeInfo()
                .GetMethod("OnValidationError", MethodFlags);

            if (method == null)
            {
                return;
            }

            var accessor = context.ModelAccessor;
            context.Application.ValidationErrorHandler = (v) =>
            {
                var arguments = ReflectionHelper.BindParameters(method, context.Application);
                var result = method.Invoke(accessor.GetModel(), arguments);
                if (method.ReturnType == typeof(int))
                {
                    return (int)result;
                }

                return CommandLineApplication.ValidationErrorExitCode;
            };
        }
    }
}
