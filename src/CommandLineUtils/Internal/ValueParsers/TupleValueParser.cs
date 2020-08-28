// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Abstractions
{
    internal class TupleValueParser
    {
        public static IValueParser<Tuple<bool, T>> Create<T>(IValueParser<T> typeParser)
        {
            if (typeParser == null) throw new ArgumentNullException(nameof(typeParser));

            return
                ValueParser.Create((argName, value, culture) =>
                    value == null
                        ? Tuple.Create<bool, T>(false, default!)
                        : Tuple.Create(true, typeParser.Parse(argName, value, culture)));
        }
    }
}
