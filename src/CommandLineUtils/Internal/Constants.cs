// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class Constants
    {
        public static readonly object[] EmptyArray
#if NET45
            = new object[0];
#elif (NETSTANDARD1_6 || NETSTANDARD2_0)
            = Array.Empty<object>();
#else
#error Update target frameworks
#endif
    }
}
