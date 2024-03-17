// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Conventions;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Invokes a method named "OnValidate" on the model type after parsing.
    /// </summary>
    public class ValidateMethodConvention : IConvention
    {
        /// <inheritdoc />
        public void Apply(ConventionContext context)
        {
            var modelAccessor = context.ModelAccessor;
            if (context.ModelType == null || modelAccessor == null)
            {
                return;
            }

            const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var method = context.ModelType.GetMethod("OnValidate", MethodFlags);
            if (method == null)
            {
                return;
            }

            if (method.ReturnType != typeof(ValidationResult))
            {
                throw new InvalidOperationException(Strings.InvalidOnValidateReturnType(context.ModelType));
            }

            var methodParams = method.GetParameters();
            context.Application.OnValidate(ctx =>
            {
                var arguments = new object[methodParams.Length];

                for (var i = 0; i < methodParams.Length; i++)
                {
                    var methodParam = methodParams[i];

                    if (typeof(ValidationContext).IsAssignableFrom(methodParam.ParameterType))
                    {
                        arguments[i] = ctx;
                    }
                    else if (typeof(CommandLineContext).IsAssignableFrom(methodParam.ParameterType))
                    {
                        arguments[i] = context.Application._context;
                    }
                    else
                    {
                        throw new InvalidOperationException(Strings.UnsupportedParameterTypeOnMethod(method.Name, methodParam));
                    }
                }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
                return (ValidationResult)method.Invoke(modelAccessor.GetModel(), arguments);
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            });
        }
    }
}
