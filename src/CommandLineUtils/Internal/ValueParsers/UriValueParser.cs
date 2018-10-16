// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    partial class StockValueParsers
    {
        public static readonly IValueParser<Uri> Uri = ValueParser.Create(
            (_, value, culture) => new Uri(value, UriKind.RelativeOrAbsolute));
    }

    internal static class UriValueParser
    {
        public static IValueParser<Uri> Singleton { get; } = StockValueParsers.Uri;
    }
}
