// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents a subcommand.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class SubcommandAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SubcommandAttribute" />.
        /// </summary>
        /// <param name="name">The name of the subcommand</param>
        /// <param name="commandType">The type of the subcommand.</param>
        public SubcommandAttribute(string name, Type commandType)
        {
            CommandType = commandType;
            Name = name;
        }

        /// <summary>
        /// The name of the subcommand.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the subcommand.
        /// </summary>
        public Type CommandType { get; set; }

        internal void Configure(CommandLineApplication app)
        {
            app.Name = Name;
        }
    }
}
