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
        private const BindingFlags InheritedLookup = BindingFlags.Public | BindingFlags.NonPublic |
                                                     BindingFlags.Instance | BindingFlags.Static;

        private const BindingFlags DeclaredOnlyLookup = InheritedLookup | BindingFlags.DeclaredOnly;

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

        public static bool IsEnumerableType(Type type, [NotNullWhen(true)] out Type? wrappedType)
        {
            if (type.IsArray)
            {
                wrappedType = type.GetElementType()!;
                return true;
            }

            if (type.IsGenericType)
            {
                wrappedType = type.GenericTypeArguments[0];
                return true;
            }

            wrappedType = null;
            return false;
        }

        private class MethodMetadataEquality : IEqualityComparer<MethodInfo>
        {
            public bool Equals(MethodInfo? x, MethodInfo? y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                return x != null && y != null && x.HasSameMetadataDefinitionAs(y);
            }

            public int GetHashCode(MethodInfo obj)
            {
                return obj.HasMetadataToken() ? obj.GetMetadataToken().GetHashCode() : 0;
            }
        }

        private static IEnumerable<MemberInfo> GetAllMembers(Type type)
        {
            // Keep track of the base definitions of property getters we see so we can skip ones we've seen already
            var baseGetters = new HashSet<MethodInfo>(new MethodMetadataEquality());

            var currentType = type;
            while (currentType != null)
            {
                var members = currentType.GetMembers(InheritedLookup);
                foreach (var member in members)
                {
                    if (member is PropertyInfo property)
                    {
                        var getter = property.GetGetMethod(true)?.GetBaseDefinition();

                        // If we have a getter, try to add it to our set. If it _wasn't_ a new element, don't yield it.
                        if (getter != null && !baseGetters.Add(getter))
                        {
                            continue;
                        }
                    }

                    yield return member;
                }

                currentType = currentType.BaseType;
            }
        }
    }
}
