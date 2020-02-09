// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

#pragma warning disable 618

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents a subcommand.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class SubcommandAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="McMaster.Extensions.CommandLineUtils.SubcommandAttribute" />.
        /// </summary>
        /// <param name="subcommands">The subcommand types.</param>
        public SubcommandAttribute(params Type[] subcommands)
        {
            if (subcommands == null)
            {
                throw new ArgumentNullException(nameof(subcommands));
            }

            if (subcommands.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(subcommands));
            }

            Types = subcommands;
        }

        /// <summary>
        /// The types of the subcommands.
        /// </summary>
        public Type[] Types { get; private set; }
    }
}
