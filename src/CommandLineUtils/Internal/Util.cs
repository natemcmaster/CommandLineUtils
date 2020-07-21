// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class Util
    {
        public static T[] EmptyArray<T>()
#if NET45
            => EmptyArrayCache<T>.Value;

        private static class EmptyArrayCache<T>
        {
            internal static readonly T[] Value = new T[0];
        }

#elif NETSTANDARD2_0
            => Array.Empty<T>();
#else
#error Update target frameworks
#endif
    }
}
