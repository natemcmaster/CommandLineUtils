// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class UriValueParser : IValueParser<Uri>
    {
        private UriValueParser()
        { }

        public static UriValueParser Singleton { get; } = new UriValueParser();

        public Type TargetType { get; } = typeof(Uri);

        public Uri Parse(string argName, string value, CultureInfo culture) => new Uri(value, UriKind.RelativeOrAbsolute);

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
