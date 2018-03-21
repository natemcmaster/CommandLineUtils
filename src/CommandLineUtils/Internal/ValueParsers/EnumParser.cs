// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    using System.Globalization;

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
                // Note: Because Enum's are a special case, this value is never used
                return _enumType;
            }
        }

        public object Parse(string argName, string value, CultureInfo culture)
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
