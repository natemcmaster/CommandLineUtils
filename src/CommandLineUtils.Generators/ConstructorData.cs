// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data for a constructor.
    /// </summary>
    internal sealed class ConstructorData
    {
        /// <summary>
        /// The parameters of the constructor.
        /// </summary>
        public List<ConstructorParameterData> Parameters { get; } = new();
    }
}
