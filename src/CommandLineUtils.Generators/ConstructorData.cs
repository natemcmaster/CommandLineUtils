// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data for a constructor parameter.
    /// </summary>
    internal sealed class ConstructorParameterData
    {
        /// <summary>
        /// The fully qualified type name of the parameter.
        /// </summary>
        public string TypeName { get; set; } = "";

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; } = "";
    }

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
