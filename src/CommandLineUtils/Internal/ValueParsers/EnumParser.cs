// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.ValueParsers
{
    internal class EnumParser : IValueParser
    {
        private readonly Type _enumType;

        public EnumParser(Type enumType)
        {
            _enumType = enumType;
        }

        public Type TargetType
        {
            get
            {
                throw new InvalidOperationException($"{nameof(NullableValueParser)} does not have a target type");
            }
        }

        public object Parse(string argName, string value)
        {
            try
            {
                return Enum.Parse(_enumType, value, ignoreCase: true);
            }
            catch
            {
                throw new FormatException($"Invalid value specified for {argName}. Allowed values are: {string.Join(", ", Enum.GetNames(_enumType))}.");
            }
        }
    }
}
