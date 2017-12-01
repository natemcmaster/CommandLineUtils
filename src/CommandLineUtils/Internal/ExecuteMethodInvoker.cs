// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils
{
    internal abstract class ExecuteMethodInvoker
    {
        protected ExecuteMethodInvoker(MethodInfo method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        protected MethodInfo Method { get; }

        public static ExecuteMethodInvoker Create(Type type)
        {
            MethodInfo method;
            MethodInfo asyncMethod;
            try
            {
                method = type.GetTypeInfo().GetMethod("OnExecute", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                asyncMethod = type.GetTypeInfo().GetMethod("OnExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
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

            if (method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<int>))
            {
                return new AsyncMethodInvoker(method);
            }
            else if (method.ReturnType == typeof(void) || method.ReturnType == typeof(int))
            {
                return new SynchronousMethodInvoker(method);
            }

            throw new InvalidOperationException(Strings.InvalidOnExecuteReturnType(method.Name));
        }
    }
}
