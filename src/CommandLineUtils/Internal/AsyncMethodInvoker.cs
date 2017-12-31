// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class AsyncMethodInvoker : ExecuteMethodInvoker
    {
        public AsyncMethodInvoker(MethodInfo method) : base(method)
        {
        }

        public async Task<int> ExecuteAsync(CommandLineContext context, BindResult bindResult)
        {
            var arguments = ReflectionHelper.BindParameters(Method, context, bindResult);

            var result = (Task)Method.Invoke(bindResult.Target, arguments);
            if (result is Task<int> intResult)
            {
                return await intResult;
            }

            await result;
            return 0;
        }
    }
}
