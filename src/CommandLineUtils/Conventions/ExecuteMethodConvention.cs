// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Sets <see cref="CommandLineApplication.Invoke"/> to call a method named
    /// <c>OnExecute</c> or <c>OnExecuteAsync</c> on the model type
    /// of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class ExecuteMethodConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var handler = CreateHandler(context, out bool supportsCancellation);

            context.Application.OnExecuteAsync(handler, supportsCancellation);
        }

        private Func<CancellationToken, Task<int>> CreateHandler(ConventionContext context, out bool supportsCancellation)
        {
            const BindingFlags binding = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var typeInfo = context.ModelType.GetTypeInfo();
            MethodInfo? method;
            MethodInfo? asyncMethod;
            try
            {
                method = typeInfo.GetMethod("OnExecute", binding);
                asyncMethod = typeInfo.GetMethod("OnExecuteAsync", binding);
            }
            catch (AmbiguousMatchException ex)
            {
                supportsCancellation = false;
                return _ => throw new InvalidOperationException(Strings.AmbiguousOnExecuteMethod, ex);
            }

            if (method != null && asyncMethod != null)
            {
                supportsCancellation = false;
                return _ => throw new InvalidOperationException(Strings.AmbiguousOnExecuteMethod);
            }

            method ??= asyncMethod;

            if (method == null)
            {
                supportsCancellation = false;
                return _ => throw new InvalidOperationException(Strings.NoOnExecuteMethodFound);
            }

            var parameters = method.GetParameters();
            supportsCancellation = parameters.Any(p => p.ParameterType == typeof(CancellationToken));

            return ct => InvokeMethodAsync(method, context, ct);
        }

        private async Task<int> InvokeMethodAsync(MethodInfo method, ConventionContext context, CancellationToken cancellationToken)
        {
            var arguments = ReflectionHelper.BindParameters(method, context.Application, cancellationToken);
            var modelAccessor = context.ModelAccessor;
            if (modelAccessor == null)
            {
                throw new InvalidOperationException(Strings.ConventionRequiresModel);
            }
            var model = modelAccessor.GetModel();

            if (method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<int>))
            {
                return await InvokeAsync(method, model, arguments);
            }
            else if (method.ReturnType == typeof(void) || method.ReturnType == typeof(int))
            {
                return Invoke(method, model, arguments);
            }

            throw new InvalidOperationException(Strings.InvalidOnExecuteReturnType(method.Name));
        }

        private async Task<int> InvokeAsync(MethodInfo method, object instance, object[] arguments)
        {
            try
            {
                var result = (Task)method.Invoke(instance, arguments);
                if (result is Task<int> intResult)
                {
                    return await intResult;
                }

                await result;
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }

            return 0;
        }

        private int Invoke(MethodInfo method, object instance, object[] arguments)
        {
            try
            {
                var result = method.Invoke(instance, arguments);
                if (method.ReturnType == typeof(int))
                {
                    return (int)result;
                }
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }

            return 0;
        }
    }
}
