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
        /// This constructor is obsolete. The recommended replacement is <see cref="SubcommandAttribute(Type[])"/>.
        /// </summary>
        /// <param name="name">The name of the subcommand</param>
        /// <param name="commandType">The type of the subcommand.</param>
        [Obsolete("[Subcommand(string, Type)] is obsolete and will be removed in a future version. " +
                  "The recommended alternative is [Subcommand(Type)]. " +
                  "See https://github.com/natemcmaster/CommandLineUtils/issues/139 for details.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SubcommandAttribute(string name, Type commandType)
        {
            CommandType = commandType;
            Name = name;
        }

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

        /// <summary>
        /// This property is obsolete and will be removed in a future version.
        /// The recommended replacement is to use <see cref="CommandAttribute"/> to set names for subcommands.
        /// <para>
        /// The name of the subcommand.
        /// </para>
        /// </summary>
        [Obsolete("This property is obsolete and will be removed in a future version. " +
                  "The recommended replacement is to use CommandAttribute to set names for subcommands.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Name { get; set; }

        /// <summary>
        /// This property is obsolete and will be replaced in a future version.
        /// The recommended replacement is <see cref="Types"/>.
        /// <para>
        /// The type of the subcommand.
        /// </para>
        /// </summary>
        [Obsolete("This property is obsolete and will be replaced in a future version. " +
                  "The recommended replacement is Types.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type CommandType
        {
            get => Types.Length > 0 ? Types[0] : null;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                Types = new[] { value };
            }
        }

        internal void Configure(CommandLineApplication app)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                app.Name = Name;
            }
        }
    }
}
