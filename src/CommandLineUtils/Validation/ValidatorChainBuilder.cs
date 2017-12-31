// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Creates a chain of validations.
    /// </summary>
    /// <remarks>
    /// Extension methods for validation should hang off type.
    /// </remarks>
    public class ValidatorChainBuilder
    {
        /// <summary>
        /// Creates an instance of <see cref="ValidatorChain"/> starting with given validation rule.
        /// </summary>
        /// <param name="rule">The validator</param>
        /// <returns>The validator executor</returns>
        public virtual ValidatorChain Build(IValidator rule) 
            => new ValidatorChain(rule);
    }
}
