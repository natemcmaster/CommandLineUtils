// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Metadata for special well-known properties (Parent, Subcommand, RemainingArguments).
    /// </summary>
    public sealed class SpecialPropertiesMetadata
    {
        /// <summary>
        /// Setter for the Parent property, if one exists.
        /// </summary>
        public Action<object, object?>? ParentSetter { get; init; }

        /// <summary>
        /// The type of the Parent property.
        /// </summary>
        public Type? ParentType { get; init; }

        /// <summary>
        /// Setter for the Subcommand property, if one exists.
        /// </summary>
        public Action<object, object?>? SubcommandSetter { get; init; }

        /// <summary>
        /// The type of the Subcommand property.
        /// </summary>
        public Type? SubcommandType { get; init; }

        /// <summary>
        /// Setter for the RemainingArguments property, if one exists.
        /// </summary>
        public Action<object, object?>? RemainingArgumentsSetter { get; init; }

        /// <summary>
        /// The type of the RemainingArguments property.
        /// </summary>
        public Type? RemainingArgumentsType { get; init; }
    }

    /// <summary>
    /// Metadata for the [HelpOption] attribute.
    /// </summary>
    public sealed class HelpOptionMetadata
    {
        /// <summary>
        /// The option template.
        /// </summary>
        public string? Template { get; init; }

        /// <summary>
        /// The description.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Whether this option is inherited by subcommands.
        /// </summary>
        public bool Inherited { get; init; }
    }

    /// <summary>
    /// Metadata for the [VersionOption] attribute.
    /// </summary>
    public sealed class VersionOptionMetadata
    {
        /// <summary>
        /// The option template.
        /// </summary>
        public string? Template { get; init; }

        /// <summary>
        /// The description.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// The version string, or null if version is provided by a member.
        /// </summary>
        public string? Version { get; init; }

        /// <summary>
        /// Delegate to get the version from a member, if applicable.
        /// </summary>
        public Func<object, string?>? VersionGetter { get; init; }
    }
}
