// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// The attribute used to determine if PrefixRootFullName should be used by default. This should only be used once per command line app.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PrefixRootFullNameAttribute : Attribute
    {
        /// <summary>
        /// Determines whether to prefix the root full name.
        /// </summary>
        public bool Prefix { get; set; }


        /// <summary>
        /// Initializes a new <see cref="PrefixRootFullNameAttribute"/>.
        /// </summary>
        public PrefixRootFullNameAttribute()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="PrefixRootFullNameAttribute"/>.
        /// </summary>
        /// <param name="prefix">Determines whether to prefix the root full name.</param>
        public PrefixRootFullNameAttribute(bool prefix)
        {
            this.Prefix = prefix;
        }

        internal void Configure(CommandLineApplication app)
        {
            app.PrefixRootFullName = this.Prefix;
        }
    }
}
