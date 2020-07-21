// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils
{
    using System.Globalization;

    /// <summary>
    /// Parses a value to Tuple{bool,} or ValueTuple{bool,}
    /// </summary>
    internal interface ITupleValueParser
    {
        object Parse(bool hasValue, string argName, string value, CultureInfo culture);
    }
}
