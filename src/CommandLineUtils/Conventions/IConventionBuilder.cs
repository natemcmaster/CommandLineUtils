// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Builds a collection of conventions.
    /// </summary>
    public interface IConventionBuilder
    {
        /// <summary>
        /// Add a convention that will be applied later.
        /// </summary>
        /// <param name="convention">The convention</param>
        IConventionBuilder AddConvention(IConvention convention);
    }
}
