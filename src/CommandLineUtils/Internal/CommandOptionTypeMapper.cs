// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class CommandOptionTypeMapper
    {
        private CommandOptionTypeMapper()
        { }

        public static CommandOptionTypeMapper Default { get; } = new CommandOptionTypeMapper();

        public bool TryGetOptionType(
            Type clrType,
            ValueParserProvider valueParsers,
            out CommandOptionType optionType)
        {
            try
            {
                optionType = GetOptionType(clrType, valueParsers);
                return true;
            }
            catch
            {
                optionType = default;
                return false;
            }
        }

        public CommandOptionType GetOptionType(Type clrType, ValueParserProvider valueParsers = null)
        {
            if (clrType == typeof(bool))
            {
                return CommandOptionType.NoValue;
            }

            if (clrType == typeof(string))
            {
                return CommandOptionType.SingleValue;
            }

            if (clrType.IsArray || typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(clrType))
            {
                return CommandOptionType.MultipleValue;
            }

            var typeInfo = clrType.GetTypeInfo();
            if (typeInfo.IsEnum)
            {
                return CommandOptionType.SingleValue;
            }

            if (typeInfo.IsGenericType)
            {
                var typeDef = typeInfo.GetGenericTypeDefinition();
                if (typeDef == typeof(Nullable<>))
                {
                    return GetOptionType(typeInfo.GetGenericArguments().First(), valueParsers);
                }

                if (typeDef == typeof(Tuple<,>) && typeInfo.GenericTypeArguments[0] == typeof(bool))
                {
                    if (GetOptionType(typeInfo.GenericTypeArguments[1], valueParsers) == CommandOptionType.SingleValue)
                    {
                        return CommandOptionType.SingleOrNoValue;
                    }
                }

                if (typeDef == typeof(ValueTuple<,>) && typeInfo.GenericTypeArguments[0] == typeof(bool))
                {
                    if (GetOptionType(typeInfo.GenericTypeArguments[1], valueParsers) == CommandOptionType.SingleValue)
                    {
                        return CommandOptionType.SingleOrNoValue;
                    }
                }
            }

            if (typeof(byte) == clrType
                || typeof(short) == clrType
                || typeof(int) == clrType
                || typeof(long) == clrType
                || typeof(ushort) == clrType
                || typeof(uint) == clrType
                || typeof(ulong) == clrType
                || typeof(float) == clrType
                || typeof(double) == clrType)
            {
                return CommandOptionType.SingleValue;
            }

            if (valueParsers?.GetParser(clrType) != null)
            {
                return CommandOptionType.SingleValue;
            }

            throw new ArgumentException("Could not determine CommandOptionType", clrType.Name);
        }
    }
}
