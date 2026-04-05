// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Validate handler that uses reflection to invoke OnValidate.
    /// </summary>
    [RequiresUnreferencedCode("Uses reflection to invoke method")]
    internal sealed class ReflectionValidateHandler : IValidateHandler
    {
        private readonly MethodInfo _method;

        public ReflectionValidateHandler(MethodInfo method)
        {
            _method = method;
        }

        public ValidationResult? Invoke(object model, ValidationContext validationContext, CommandLineContext commandContext)
        {
            var parameters = _method.GetParameters();
            var args = new object?[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                if (typeof(ValidationContext).IsAssignableFrom(paramType))
                {
                    args[i] = validationContext;
                }
                else if (typeof(CommandLineContext).IsAssignableFrom(paramType))
                {
                    args[i] = commandContext;
                }
            }

            try
            {
                return (ValidationResult?)_method.Invoke(model, args);
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                throw; // Never reached
            }
        }
    }
}
