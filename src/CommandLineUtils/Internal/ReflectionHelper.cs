// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class ReflectionHelper
    {
        public static MethodInfo GetExecuteMethod<T>()
        {
            var methods = typeof(T).GetRuntimeMethods().Where(m => m.Name == "OnExecute").ToArray();
            
            if (methods.Length > 1)
            {
                throw new InvalidOperationException(Strings.AmbiguousOnExecuteMethod);
            }
            
            if (methods.Length == 0)
            {
                throw new InvalidOperationException(Strings.NoOnExecuteMethodFound);
            }

            var method = methods[0];

            if (method.ReturnType != typeof(void) && method.ReturnType != typeof(int))
            {
                throw new InvalidOperationException(Strings.InvalidOnExecuteReturnType);
            }

            return method;
        }
    }
}