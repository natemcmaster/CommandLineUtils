// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Metadata about a command option, extracted at compile time or via reflection.
    /// </summary>
    public sealed class OptionMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionMetadata"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property this option maps to.</param>
        /// <param name="propertyType">The CLR type of the property.</param>
        /// <param name="getter">Delegate to get the property value from a model instance.</param>
        /// <param name="setter">Delegate to set the property value on a model instance.</param>
        public OptionMetadata(
            string propertyName,
            Type propertyType,
            Func<object, object?> getter,
            Action<object, object?> setter)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
            Setter = setter ?? throw new ArgumentNullException(nameof(setter));
        }

        /// <summary>
        /// The name of the property this option maps to.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The CLR type of the property.
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// The type that declares this property.
        /// </summary>
        public Type? DeclaringType { get; init; }

        /// <summary>
        /// The option template (e.g., "-n|--name &lt;VALUE&gt;").
        /// </summary>
        public string? Template { get; init; }

        /// <summary>
        /// The short name (e.g., "n").
        /// </summary>
        public string? ShortName { get; init; }

        /// <summary>
        /// The long name (e.g., "name").
        /// </summary>
        public string? LongName { get; init; }

        /// <summary>
        /// The symbol name (e.g., "?").
        /// </summary>
        public string? SymbolName { get; init; }

        /// <summary>
        /// The name of value(s) shown in help text.
        /// </summary>
        public string? ValueName { get; init; }

        /// <summary>
        /// A description of this option.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// The option type.
        /// </summary>
        public CommandOptionType OptionType { get; init; }

        /// <summary>
        /// Whether the OptionType was explicitly set in the attribute (vs. inferred from property type).
        /// If true, skip parser validation at convention-apply time since a custom parser may be added later.
        /// </summary>
        public bool OptionTypeExplicitlySet { get; init; }

        /// <summary>
        /// Whether this option appears in generated help text.
        /// </summary>
        public bool ShowInHelpText { get; init; } = true;

        /// <summary>
        /// Whether subcommands should also have access to this option.
        /// </summary>
        public bool Inherited { get; init; }

        /// <summary>
        /// Validation attributes applied to this option.
        /// </summary>
        public IReadOnlyList<ValidationAttribute> Validators { get; init; } = Array.Empty<ValidationAttribute>();

        /// <summary>
        /// Delegate to get the property value from a model instance.
        /// </summary>
        public Func<object, object?> Getter { get; }

        /// <summary>
        /// Delegate to set the property value on a model instance.
        /// </summary>
        public Action<object, object?> Setter { get; }
    }
}
