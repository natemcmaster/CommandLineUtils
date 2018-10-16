// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    partial class StockValueParsers
    {
        public static readonly IValueParser<string> String = ValueParser.Create(
            (_, value, __) => value);
    }

    internal static class StringValueParser
    {
        public static IValueParser<string> Singleton { get; } = StockValueParsers.String;
    }
}
