// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Creates a collection of validators for <see cref="CommandArgument"/>.
    /// </summary>
    /// <remarks>
    /// Custom validation extension methods that only apply to <see cref="CommandArgument"/> should hang off this type.
    /// </remarks>
    public interface IArgumentValidationBuilder : IValidationBuilder
    {
        /// <summary>
        /// Use the given <see cref="IArgumentValidator"/>.
        /// </summary>
        /// <param name="validator">The validator.</param>
        void Use(IArgumentValidator validator);
    }
}
