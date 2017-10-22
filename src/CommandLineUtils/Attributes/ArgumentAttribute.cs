// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents one or many positional command line arguments. 
    /// Arguments are parsed based the <see cref="Order"/> given.
    /// Compare to <seealso cref="OptionAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ArgumentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="ArgumentAttribute" />.
        /// </summary>
        /// <param name="order">The order</param>
        public ArgumentAttribute(int order)
            : this(order, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="ArgumentAttribute" />.
        /// </summary>
        /// <param name="order">The order</param>
        /// <param name="name">The name</param>
        public ArgumentAttribute(int order, string name)
        {
            Order = order;
            Name = name;
        }

        /// <summary>
        /// The order in which the argument is expected, relative to other arguments.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The name of the argument. <seealso cref="CommandArgument.Name"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Determines if the argument appears in the generated help-text.  <seealso cref="CommandArgument.ShowInHelpText"/>.
        /// </summary>
        public bool ShowInHelpText { get; set; } = true;

        /// <summary>
        /// A description of the argument.  <seealso cref="CommandArgument.Description"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Allow multiple values. <seealso cref="CommandArgument.MultipleValues"/>.
        /// </summary>
        public bool MultipleValues { get; set; }
    }
}
