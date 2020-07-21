// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Defines a convention that is implemented as an attribute on a model type.
    /// </summary>
    public interface IMemberConvention
    {
        /// <summary>
        /// Apply the convention given a property or method.
        /// </summary>
        /// <param name="context">The convention context.</param>
        /// <param name="member">A member of the model type to which the attribute is applied.</param>
        void Apply(ConventionContext context, MemberInfo member);
    }
}
