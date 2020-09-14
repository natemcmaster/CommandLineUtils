// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class NullableValueParser : IValueParser
    {
        private readonly IValueParser _wrapped;

        public NullableValueParser(IValueParser boxedParser)
        {
            _wrapped = boxedParser;
        }

        public Type TargetType
        {
            get
            {
                throw new InvalidOperationException($"{nameof(NullableValueParser)} does not have a target type");
            }
        }


        public object? Parse(string? argName, string? value, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value)
                ? _wrapped.Parse(argName, value, culture)
                : null;
        }
    }
}
