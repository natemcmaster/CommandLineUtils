// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class ReflectionHelper
    {
        public static SetPropertyDelegate GetPropertySetter(PropertyInfo prop)
        {
            var setter = prop.GetSetMethod(nonPublic: true);
            if (setter != null)
            {
                return (obj, value) => setter.Invoke(obj, new object?[] { value });
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

        public static MethodInfo[] GetPropertyOrMethod(Type type, string name)
        {
            var members = GetAllMembers(type.GetTypeInfo()).ToList();
            return members
                .OfType<MethodInfo>()
                .Where(m => m.Name == name)
                .Concat(members.OfType<PropertyInfo>().Where(m => m.Name == name).Select(p => p.GetMethod))
                .Where(m => m.ReturnType == typeof(string) && m.GetParameters().Length == 0)
                .ToArray();
        }

        public static PropertyInfo[] GetProperties(Type type)
        {
            return GetAllMembers(type.GetTypeInfo())
                .OfType<PropertyInfo>()
                .ToArray();
        }

        public static MemberInfo[] GetMembers(Type type)
        {
            return GetAllMembers(type.GetTypeInfo()).ToArray();
        }

        public static object[] BindParameters(MethodInfo method, CommandLineApplication command, CancellationToken cancellationToken)
        {
            var methodParams = method.GetParameters();
            var arguments = new object[methodParams.Length];

            for (var i = 0; i < methodParams.Length; i++)
            {
                var methodParam = methodParams[i];

                if (typeof(CommandLineApplication).GetTypeInfo().IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = command;
                }
                else if (typeof(IConsole).GetTypeInfo().IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = command._context.Console;
                }
                else if (typeof(ValidationResult).GetTypeInfo().IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = command.GetValidationResult();
                }
                else if (typeof(CommandLineContext).GetTypeInfo().IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = command._context;
                }
                else if (typeof(CancellationToken) == methodParam.ParameterType && cancellationToken != CancellationToken.None)
                {
                    arguments[i] = cancellationToken;
                }
                else
                {
                    object? service = command.AdditionalServices?.GetService(methodParam.ParameterType);
                    arguments[i] = service ?? throw new InvalidOperationException(Strings.UnsupportedParameterTypeOnMethod(method.Name, methodParam));
                }
            }

            return arguments;
        }

        public static bool IsNullableType(TypeInfo typeInfo, out Type? wrappedType)
        {
            var result = typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
            wrappedType = result ? typeInfo.GetGenericArguments().First() : null;

            return result;
        }

        static IEnumerable<MemberInfo> GetAllMembers(TypeInfo typeInfo)
        {
            const BindingFlags binding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

            while (typeInfo != null) 
            {
                var members = typeInfo.GetMembers(binding);
                foreach (var member in members)
                {
                    yield return member;
                }

                typeInfo = typeInfo.BaseType?.GetTypeInfo();
            }
        }
    }
}
