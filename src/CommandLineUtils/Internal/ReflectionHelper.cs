// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    internal static class ReflectionHelper
    {
        private const BindingFlags DeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static SetPropertyDelegate GetPropertySetter(PropertyInfo prop)
        {
            var setter = prop.GetSetMethod(nonPublic: true);
            if (setter != null)
            {
                return (obj, value) => setter.Invoke(obj, new object?[] { value });
            }
            else
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var backingField = prop.DeclaringType.GetField($"<{prop.Name}>k__BackingField", DeclaredOnlyLookup);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (backingField == null)
                {
                    throw new InvalidOperationException(
                        $"Could not find a way to set {prop.DeclaringType.FullName}.{prop.Name}. Try adding a private setter.");
                }

                return (obj, value) => backingField.SetValue(obj, value);
            }
        }

        public static GetPropertyDelegate GetPropertyGetter(PropertyInfo prop)
        {
            var getter = prop.GetGetMethod(nonPublic: true);
            if (getter != null)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return obj => getter.Invoke(obj, Array.Empty<object>());
#pragma warning restore CS8603 // Possible null reference return.
            }
            else
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var backingField = prop.DeclaringType.GetField($"<{prop.Name}>k__BackingField", DeclaredOnlyLookup);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (backingField == null)
                {
                    throw new InvalidOperationException(
                        $"Could not find a way to get {prop.DeclaringType.FullName}.{prop.Name}. Try adding a getter.");
                }

#pragma warning disable CS8603 // Possible null reference return.
                return obj => backingField.GetValue(obj);
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        public static MethodInfo[] GetPropertyOrMethod(Type type, string name)
        {
            var members = GetAllMembers(type).ToList();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return members
                .OfType<MethodInfo>()
                .Where(m => m.Name == name)
                .Concat(members.OfType<PropertyInfo>().Where(m => m.Name == name).Select(p => p.GetMethod))
                .Where(m => m.ReturnType == typeof(string) && m.GetParameters().Length == 0)
                .ToArray();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public static PropertyInfo[] GetProperties(Type type)
        {
            return GetAllMembers(type)
                .OfType<PropertyInfo>()
                .ToArray();
        }

        public static MemberInfo[] GetMembers(Type type)
        {
            return GetAllMembers(type).ToArray();
        }

        public static object?[] BindParameters(MethodInfo method, CommandLineApplication command, CancellationToken cancellationToken)
        {
            var methodParams = method.GetParameters();
            var arguments = new object?[methodParams.Length];

            for (var i = 0; i < methodParams.Length; i++)
            {
                var methodParam = methodParams[i];

                if (typeof(CommandLineApplication).IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = command;
                }
                else if (typeof(IConsole).IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = command._context.Console;
                }
                else if (typeof(ValidationResult).IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = command.GetValidationResult();
                }
                else if (typeof(CommandLineContext).IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = command._context;
                }
                else if (typeof(CancellationToken) == methodParam.ParameterType && cancellationToken != CancellationToken.None)
                {
                    arguments[i] = cancellationToken;
                }
                else
                {
                    var service = command.AdditionalServices?.GetService(methodParam.ParameterType);
                    arguments[i] = service ?? throw new InvalidOperationException(Strings.UnsupportedParameterTypeOnMethod(method.Name, methodParam));
                }
            }

            return arguments;
        }

        public static bool IsNullableType(Type type, [NotNullWhen(true)] out Type? wrappedType)
        {
            var result = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            wrappedType = result ? type.GetGenericArguments().First() : null;

            return result;
        }

        public static bool IsSpecialValueTupleType(Type type, [NotNullWhen(true)] out Type? wrappedType)
        {
            var result = type.IsGenericType &&
                         type.GetGenericTypeDefinition() == typeof(ValueTuple<,>) &&
                         type.GenericTypeArguments[0] == typeof(bool);
            wrappedType = result ? type.GenericTypeArguments[1] : null;

            return result;
        }

        public static bool IsSpecialTupleType(Type type, [NotNullWhen(true)] out Type? wrappedType)
        {
            var result = type.IsGenericType &&
                         type.GetGenericTypeDefinition() == typeof(Tuple<,>) &&
                         type.GenericTypeArguments[0] == typeof(bool);
            wrappedType = result ? type.GenericTypeArguments[1] : null;

            return result;
        }

        private static IEnumerable<MemberInfo> GetAllMembers(Type type)
        {
            while (type != null)
            {
                var members = type.GetMembers(DeclaredOnlyLookup);
                foreach (var member in members)
                {
                    yield return member;
                }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                type = type.BaseType;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
        }
    }
}
