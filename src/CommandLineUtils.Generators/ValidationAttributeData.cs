// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data for a validation attribute applied to a property.
    /// </summary>
    internal sealed class ValidationAttributeData
    {
        /// <summary>
        /// The fully qualified type name of the validation attribute.
        /// </summary>
        public string TypeName { get; set; } = "";

        /// <summary>
        /// Constructor arguments for the validation attribute, as code strings.
        /// </summary>
        public List<string> ConstructorArguments { get; } = new List<string>();

        /// <summary>
        /// Named property assignments for the validation attribute.
        /// Key is property name, value is the code string for the value.
        /// </summary>
        public Dictionary<string, string> NamedArguments { get; } = new Dictionary<string, string>();
    }
}
