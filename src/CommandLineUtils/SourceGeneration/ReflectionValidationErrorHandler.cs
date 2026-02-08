// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Validation error handler that uses reflection to invoke OnValidationError.
    /// </summary>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Uses reflection to invoke method")]
#elif NET472_OR_GREATER
#else
#error Target framework misconfiguration
#endif
    internal sealed class ReflectionValidationErrorHandler : IValidationErrorHandler
    {
        private readonly MethodInfo _method;

        public ReflectionValidationErrorHandler(MethodInfo method)
        {
            _method = method;
        }

        public int Invoke(object model, ValidationResult validationResult)
        {
            var parameters = _method.GetParameters();
            var args = new object?[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                if (typeof(ValidationResult).IsAssignableFrom(paramType))
                {
                    args[i] = validationResult;
                }
            }

            try
            {
                var result = _method.Invoke(model, args);
                if (_method.ReturnType == typeof(int) && result != null)
                {
                    return (int)result;
                }
                return 1; // Default error code
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                throw; // Never reached
            }
        }
    }
}
