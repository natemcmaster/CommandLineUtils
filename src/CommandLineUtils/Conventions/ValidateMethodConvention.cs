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
            if (context.ModelType == null)
            {
                return;
            }

            const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var method = context.ModelType
                .GetTypeInfo()
                .GetMethod("OnValidate", MethodFlags);

            if (method == null)
            {
                return;
            }

            if (method.ReturnType != typeof(ValidationResult))
            {
                throw new InvalidOperationException(Strings.InvalidOnValidateReturnType(context.ModelType));
            }

            var accessor = context.ModelAccessor;
            var methodParams = method.GetParameters();
            context.Application.OnValidate(ctx =>
            {
                var arguments = new object[methodParams.Length];

                for (var i = 0; i < methodParams.Length; i++)
                {
                    var methodParam = methodParams[i];

                    if (typeof(ValidationContext).GetTypeInfo().IsAssignableFrom(methodParam.ParameterType))
                    {
                        arguments[i] = ctx;
                    }
                    else if (typeof(CommandLineContext).GetTypeInfo().IsAssignableFrom(methodParam.ParameterType))
                    {
                        arguments[i] = context.Application._context;
                    }
                    else
                    {
                        throw new InvalidOperationException(Strings.UnsupportedParameterTypeOnMethod(method.Name, methodParam));
                    }
                }

                return (ValidationResult)method.Invoke(accessor.GetModel(), arguments);
            });
        }
    }
}
