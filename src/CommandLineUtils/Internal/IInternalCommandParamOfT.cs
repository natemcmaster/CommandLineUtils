// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils
{
    internal interface IInternalCommandParamOfT
    {
        void Parse(CultureInfo culture);
    }
}
