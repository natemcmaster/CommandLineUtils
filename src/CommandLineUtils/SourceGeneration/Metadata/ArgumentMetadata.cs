// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Metadata about a command argument, extracted at compile time or via reflection.
    /// </summary>
    public sealed class ArgumentMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentMetadata"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property this argument maps to.</param>
        /// <param name="propertyType">The CLR type of the property.</param>
        /// <param name="order">The order in which the argument is expected.</param>
        /// <param name="getter">Delegate to get the property value from a model instance.</param>
        /// <param name="setter">Delegate to set the property value on a model instance.</param>
        public ArgumentMetadata(
            string propertyName,
            Type propertyType,
            int order,
            Func<object, object?> getter,
            Action<object, object?> setter)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            Order = order;
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
            Setter = setter ?? throw new ArgumentNullException(nameof(setter));
        }

        /// <summary>
        /// The name of the property this argument maps to.
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
        /// The order in which the argument is expected, relative to other arguments.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The name of the argument.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// A description of the argument.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Whether this argument appears in generated help text.
        /// </summary>
        public bool ShowInHelpText { get; init; } = true;

        /// <summary>
        /// Whether this argument accepts multiple values.
        /// </summary>
        public bool MultipleValues { get; init; }

        /// <summary>
        /// Validation attributes applied to this argument.
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
