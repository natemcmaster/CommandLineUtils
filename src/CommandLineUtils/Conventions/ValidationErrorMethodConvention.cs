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
            var modelAccessor = context.ModelAccessor;
            if (context.ModelType == null || modelAccessor == null)
            {
                return;
            }

            const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var method = context.ModelType.GetMethod("OnValidationError", MethodFlags);
            if (method == null)
            {
                return;
            }

            context.Application.ValidationErrorHandler = (v) =>
            {
                var arguments = ReflectionHelper.BindParameters(method, context.Application, default);
                var result = method.Invoke(modelAccessor.GetModel(), arguments);
                if (method.ReturnType == typeof(int))
                {
#pragma warning disable CS8605 // Unboxing a possibly null value.
                    return (int)result;
#pragma warning restore CS8605 // Unboxing a possibly null value.
                }

                return CommandLineApplication.ValidationErrorExitCode;
            };
        }
    }
}
