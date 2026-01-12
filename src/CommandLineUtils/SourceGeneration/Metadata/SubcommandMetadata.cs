// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Metadata about a subcommand, extracted at compile time or via reflection.
    /// </summary>
    public sealed class SubcommandMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubcommandMetadata"/> class.
        /// </summary>
        /// <param name="subcommandType">The type of the subcommand.</param>
        public SubcommandMetadata(Type subcommandType)
        {
            SubcommandType = subcommandType ?? throw new ArgumentNullException(nameof(subcommandType));
        }

        /// <summary>
        /// The type of the subcommand.
        /// </summary>
        public Type SubcommandType { get; }

        /// <summary>
        /// The command name (if specified, otherwise derived from type name).
        /// </summary>
        public string? CommandName { get; init; }

        /// <summary>
        /// Factory to get the metadata provider for the subcommand type.
        /// This avoids reflection when the subcommand also has generated metadata.
        /// </summary>
        public Func<ICommandMetadataProvider>? MetadataProviderFactory { get; init; }
    }
}
