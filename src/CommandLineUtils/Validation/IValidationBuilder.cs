// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Creates a collection of validators.
    /// </summary>
    /// <remarks>
    /// Custom validation extension methods should hang off this type.
    /// </remarks>
    public interface IValidationBuilder
    {
        /// <summary>
        /// Use the <see cref="IValidator"/>.
        /// </summary>
        /// <param name="validator">The validator.</param>
        void Use(IValidator validator);
    }
}
