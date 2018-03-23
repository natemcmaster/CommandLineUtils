// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class EnumParser : IValueParser
    {
        public EnumParser(Type enumType)
        {
            TargetType = enumType;
        }

        public Type TargetType { get; }

        public object Parse(string argName, string value, CultureInfo culture)
        {
            if (value == null) return Enum.ToObject(TargetType, 0);

            try
            {
                return Enum.Parse(TargetType, value, ignoreCase: true);
            }
            catch
            {
                throw new FormatException($"Invalid value specified for {argName}. Allowed values are: {string.Join(", ", Enum.GetNames(TargetType))}.");
            }
        }
    }
}
