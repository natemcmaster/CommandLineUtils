// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ArgumentAttribute : Attribute
    {
        public ArgumentAttribute(int order)
            : this(order, null)
        { }

        public ArgumentAttribute(int order, string name)
        {
            Order = order;
            Name = name;
        }

        public string Name { get; set; }

        public int Order { get; set; }

        public bool MultipleValues { get; set; }
        public string Description { get; internal set; }
        public bool ShowInHelpText { get; internal set; }
    }
}
