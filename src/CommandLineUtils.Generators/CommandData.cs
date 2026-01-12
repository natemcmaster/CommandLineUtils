// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data about a command class.
    /// </summary>
    internal sealed class CommandData
    {
        public INamedTypeSymbol TypeSymbol { get; set; } = null!;
        public string FullTypeName { get; set; } = "";
        public string Namespace { get; set; } = "";
        public string ClassName { get; set; } = "";
        /// <summary>
        /// The inferred command name (kebab-case, minus "Command" suffix).
        /// </summary>
        public string InferredName { get; set; } = "";
        public CommandAttributeData CommandAttribute { get; set; } = new();
        public List<OptionData> Options { get; } = new();
        public List<ArgumentData> Arguments { get; } = new();
        public List<SubcommandData> Subcommands { get; } = new();
        public HelpOptionData? HelpOption { get; set; }
        public VersionOptionData? VersionOption { get; set; }
        public SpecialPropertiesData SpecialProperties { get; set; } = new();
        public bool HasOnExecute { get; set; }
        public bool OnExecuteIsAsync { get; set; }
        public bool OnExecuteReturnsInt { get; set; }
        public bool OnExecuteHasAppParameter { get; set; }
        public bool OnExecuteHasCancellationToken { get; set; }
        public bool HasOnValidate { get; set; }
        public bool HasOnValidationError { get; set; }

        /// <summary>
        /// Public constructors of the command class, ordered by parameter count descending.
        /// </summary>
        public List<ConstructorData> Constructors { get; } = new();
    }
}
