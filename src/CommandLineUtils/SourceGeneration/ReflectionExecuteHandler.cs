// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Execute handler that uses reflection to invoke OnExecute/OnExecuteAsync.
    /// </summary>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Uses reflection to invoke method")]
#endif
    internal sealed class ReflectionExecuteHandler : IExecuteHandler
    {
        private readonly MethodInfo _method;

        public ReflectionExecuteHandler(MethodInfo method, bool isAsync)
        {
            _method = method;
            IsAsync = isAsync;
        }

        public bool IsAsync { get; }

        public async Task<int> InvokeAsync(object model, CommandLineApplication app, CancellationToken cancellationToken)
        {
            var arguments = ReflectionHelper.BindParameters(_method, app, cancellationToken);

            if (_method.ReturnType == typeof(Task) || _method.ReturnType == typeof(Task<int>))
            {
                return await InvokeAsyncMethod(model, arguments);
            }
            else if (_method.ReturnType == typeof(void) || _method.ReturnType == typeof(int))
            {
                return InvokeSyncMethod(model, arguments);
            }

            throw new System.InvalidOperationException(Strings.InvalidOnExecuteReturnType(_method.Name));
        }

        private async Task<int> InvokeAsyncMethod(object instance, object?[] arguments)
        {
            try
            {
                var result = (Task?)_method.Invoke(instance, arguments);
                if (result is Task<int> intResult)
                {
                    return await intResult;
                }

                if (result != null)
                {
                    await result;
                }
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }

            return 0;
        }

        private int InvokeSyncMethod(object instance, object?[] arguments)
        {
            try
            {
                var result = _method.Invoke(instance, arguments);
                if (_method.ReturnType == typeof(int) && result != null)
                {
                    return (int)result;
                }
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }

            return 0;
        }
    }
}
