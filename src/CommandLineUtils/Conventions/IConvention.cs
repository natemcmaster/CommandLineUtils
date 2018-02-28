// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Defines a convention for an instance of <see cref="CommandLineApplication{TModel}" />.
    /// </summary>
    public interface IConvention
    {
        /// <summary>
        /// Apply the convention
        /// </summary>
        /// <param name="context">The context in which the convention is applied.</param>
        void Apply(ConventionContext context);
    }
}
