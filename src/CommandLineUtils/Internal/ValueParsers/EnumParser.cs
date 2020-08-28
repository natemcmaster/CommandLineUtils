// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal static class EnumParser
    {
        public static IValueParser Create(Type enumType) =>
            ValueParser.Create(enumType, (argName, value, culture) =>
            {
                if (value == null) return Enum.ToObject(enumType, 0);

                try
                {
                    return Enum.Parse(enumType, value, ignoreCase: true);
                }
                catch
                {
                    throw new FormatException(
                        $"Invalid value specified for {argName}. Allowed values are: {string.Join(", ", Enum.GetNames(enumType))}.");
                }
            });
    }
}
