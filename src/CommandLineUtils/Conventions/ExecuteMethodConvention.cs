// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

            context.Application.OnExecute(async cancellationToken => await OnExecute(context, cancellationToken));
        }

        private async Task<int> OnExecute(ConventionContext context, CancellationToken cancellationToken)
        {
            const BindingFlags binding = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var typeInfo = context.ModelType.GetTypeInfo();
            MethodInfo method;
            MethodInfo asyncMethod;
            try
            {
                method = typeInfo.GetMethod("OnExecute", binding);
                asyncMethod = typeInfo.GetMethod("OnExecuteAsync", binding);
            }
            catch (AmbiguousMatchException ex)
            {
                throw new InvalidOperationException(Strings.AmbiguousOnExecuteMethod, ex);
            }

            if (method != null && asyncMethod != null)
            {
                throw new InvalidOperationException(Strings.AmbiguousOnExecuteMethod);
            }

            method = method ?? asyncMethod;

            if (method == null)
            {
                throw new InvalidOperationException(Strings.NoOnExecuteMethodFound);
            }

            var arguments = ReflectionHelper.BindParameters(method, context.Application, cancellationToken);
            var model = context.ModelAccessor.GetModel();

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
                var result = (Task) method.Invoke(instance, arguments);
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
                    return (int) result;
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
