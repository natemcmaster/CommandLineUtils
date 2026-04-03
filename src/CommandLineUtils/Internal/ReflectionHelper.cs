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

        // Cached reflection handles for keyed DI support (available in .NET 8+ when
        // Microsoft.Extensions.DependencyInjection.Abstractions is present at runtime).
        // By resolving these via reflection the core library avoids a hard dependency on
        // the DI abstractions package.
        private static readonly Type? s_fromKeyedServicesAttributeType;
        private static readonly PropertyInfo? s_keyProperty;
        private static readonly Type? s_keyedServiceProviderType;
        private static readonly MethodInfo? s_getKeyedServiceMethod;

        static ReflectionHelper()
        {
            s_fromKeyedServicesAttributeType = Type.GetType(
                "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute, Microsoft.Extensions.DependencyInjection.Abstractions");

            if (s_fromKeyedServicesAttributeType != null)
            {
                s_keyProperty = s_fromKeyedServicesAttributeType.GetProperty("Key");
            }

            s_keyedServiceProviderType = Type.GetType(
                "Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider, Microsoft.Extensions.DependencyInjection.Abstractions");

            if (s_keyedServiceProviderType != null)
            {
                s_getKeyedServiceMethod = s_keyedServiceProviderType.GetMethod(
                    "GetKeyedService",
                    new[] { typeof(Type), typeof(object) });
            }
        }

        public static SetPropertyDelegate GetPropertySetter(PropertyInfo prop)
        {
            var setter = prop.GetSetMethod(nonPublic: true);
            if (setter != null)
            {
                return (obj, value) => setter.Invoke(obj, [value]);
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
                return obj => getter.Invoke(obj, []);
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
                    if (TryResolveKeyedService(methodParam, command, out var keyedService))
                    {
                        arguments[i] = keyedService;
                    }
                    else
                    {
                        var service = command.AdditionalServices?.GetService(methodParam.ParameterType);
                        arguments[i] = service ?? throw new InvalidOperationException(Strings.UnsupportedParameterTypeOnMethod(method.Name, methodParam));
                    }
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

        private static bool TryResolveKeyedService(
            ParameterInfo parameter,
            CommandLineApplication command,
            out object? service)
        {
            service = null;

            // If keyed services types aren't available at runtime, skip
            if (s_fromKeyedServicesAttributeType == null ||
                s_keyProperty == null ||
                s_keyedServiceProviderType == null ||
                s_getKeyedServiceMethod == null)
            {
                return false;
            }

            // Check if parameter has [FromKeyedServices] attribute
            var keyedAttr = parameter.GetCustomAttribute(s_fromKeyedServicesAttributeType);
            if (keyedAttr == null)
            {
                return false;
            }

            // Get the key from the attribute
            var key = s_keyProperty.GetValue(keyedAttr);

            // Check if the service provider supports keyed services
            if (command.AdditionalServices == null ||
                !s_keyedServiceProviderType.IsInstanceOfType(command.AdditionalServices))
            {
                throw new InvalidOperationException(
                    $"Parameter '{parameter.Name}' has [FromKeyedServices] attribute, " +
                    "but AdditionalServices does not implement IKeyedServiceProvider. " +
                    "Ensure you're using a DI container that supports keyed services (.NET 8+).");
            }

            // Invoke GetKeyedService via reflection
            service = s_getKeyedServiceMethod.Invoke(
                command.AdditionalServices,
                new[] { parameter.ParameterType, key });

            if (service == null)
            {
                throw new InvalidOperationException(
                    $"No keyed service found for type '{parameter.ParameterType}' " +
                    $"with key '{key}'.");
            }

            return true;
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
