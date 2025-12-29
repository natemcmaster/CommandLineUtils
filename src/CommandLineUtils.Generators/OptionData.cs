// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data for an option property.
    /// </summary>
    internal sealed class OptionData
    {
        public string PropertyName { get; set; } = "";
        public string PropertyType { get; set; } = "";
        public string? Template { get; set; }
        public string? ShortName { get; set; }
        public string? LongName { get; set; }
        public string? SymbolName { get; set; }
        public string? ValueName { get; set; }
        public string? Description { get; set; }
        public bool? ShowInHelpText { get; set; }
        public bool? Inherited { get; set; }
        public int? OptionType { get; set; }
        /// <summary>
        /// True if OptionType was explicitly set in the attribute, false if inferred from property type.
        /// </summary>
        public bool OptionTypeExplicitlySet { get; set; }
        /// <summary>
        /// The inferred OptionType based on property type (used when OptionType is not explicitly set).
        /// </summary>
        public int InferredOptionType { get; set; }
        /// <summary>
        /// Validation attributes applied to this option.
        /// </summary>
        public List<ValidationAttributeData> Validators { get; } = new List<ValidationAttributeData>();
    }
}
