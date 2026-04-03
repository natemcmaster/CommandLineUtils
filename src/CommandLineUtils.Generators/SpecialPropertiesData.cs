// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data for special properties (Parent, Subcommand, RemainingArguments).
    /// </summary>
    internal sealed class SpecialPropertiesData
    {
        /// <summary>
        /// The name of the Parent property, if one exists.
        /// </summary>
        public string? ParentPropertyName { get; set; }

        /// <summary>
        /// The type of the Parent property.
        /// </summary>
        public string? ParentPropertyType { get; set; }

        /// <summary>
        /// The name of the Subcommand property, if one exists.
        /// </summary>
        public string? SubcommandPropertyName { get; set; }

        /// <summary>
        /// The type of the Subcommand property.
        /// </summary>
        public string? SubcommandPropertyType { get; set; }

        /// <summary>
        /// The name of the RemainingArguments property, if one exists.
        /// </summary>
        public string? RemainingArgumentsPropertyName { get; set; }

        /// <summary>
        /// The type of the RemainingArguments property.
        /// </summary>
        public string? RemainingArgumentsPropertyType { get; set; }

        /// <summary>
        /// Whether the RemainingArguments property is an array type (vs IReadOnlyList, IEnumerable, etc).
        /// </summary>
        public bool RemainingArgumentsIsArray { get; set; }

        /// <summary>
        /// Whether any special properties exist.
        /// </summary>
        public bool HasAny =>
            ParentPropertyName != null ||
            SubcommandPropertyName != null ||
            RemainingArgumentsPropertyName != null;
    }
}
