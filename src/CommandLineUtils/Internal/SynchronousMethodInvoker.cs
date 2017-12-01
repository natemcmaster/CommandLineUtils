// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class SynchronousMethodInvoker : ExecuteMethodInvoker
    {
        public SynchronousMethodInvoker(MethodInfo method) : base(method)
        {
        }

        public int Execute(IConsole console, BindContext bindResult)
        {
            var arguments = ReflectionHelper.BindParameters(Method, console, bindResult);

            var result = Method.Invoke(bindResult.Target, arguments);
            if (Method.ReturnType == typeof(int))
            {
                return (int)result;
            }

            return 0;
        }
    }
}
