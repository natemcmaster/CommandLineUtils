// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class ReflectionHelper
    {
        public static MethodInfo GetExecuteMethod<T>(bool async)
            => GetExecuteMethod(typeof(T), async);

        public static MethodInfo GetExecuteMethod(Type type, bool async)
        {
            MethodInfo method;
            try
            {
                method = type.GetTypeInfo().GetMethod("OnExecute", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (method == null && async)
                {
                    method = type.GetTypeInfo().GetMethod("OnExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                }
            }
            catch (AmbiguousMatchException ex)
            {
                throw new InvalidOperationException(Strings.AmbiguousOnExecuteMethod, ex);
            }

            if (method == null)
            {
                throw new InvalidOperationException(Strings.NoOnExecuteMethodFound);
            }

            if (async)
            {
                if (method.ReturnType != typeof(Task) && method.ReturnType != typeof(Task<int>))
                {
                    throw new InvalidOperationException(Strings.InvalidAsyncOnExecuteReturnType);
                }
            }
            else if (method.ReturnType != typeof(void) && method.ReturnType != typeof(int))
            {
                throw new InvalidOperationException(Strings.InvalidOnExecuteReturnType);
            }

            return method;
        }

        public static SetPropertyDelegate GetPropertySetter(PropertyInfo prop)
        {
            var setter = prop.GetSetMethod(nonPublic: true);
            if (setter != null)
            {
                return (obj, value) => setter.Invoke(obj, new object[] { value });
            }
            else
            {
                var backingFieldName = string.Format("<{0}>k__BackingField", prop.Name);
                var backingField = prop.DeclaringType.GetTypeInfo().GetDeclaredField(backingFieldName);
                if (backingField == null)
                {
                    throw new InvalidOperationException(
                        $"Could not find a way to set {prop.DeclaringType.FullName}.{prop.Name}");
                }

                return (obj, value) => backingField.SetValue(obj, value);
            }
        }
    }
}
