// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data for an argument property.
    /// </summary>
    internal sealed class ArgumentData
    {
        public string PropertyName { get; set; } = "";
        public string PropertyType { get; set; } = "";
        public int Order { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? ShowInHelpText { get; set; }
        /// <summary>
        /// Validation attributes applied to this argument.
        /// </summary>
        public List<ValidationAttributeData> Validators { get; } = new List<ValidationAttributeData>();
    }
}
