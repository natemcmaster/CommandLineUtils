// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils
{
    internal static class DictionaryExtensions
    {
#if NET6_0_OR_GREATER
#elif NET472_OR_GREATER
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }
            dictionary.Add(key, value);
            return true;
        }
#else
#error Target framework misconfiguration
#endif
    }
}
