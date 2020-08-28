// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Creates a collection of validators on <see cref="CommandOption{T}" /> or <see cref="CommandArgument{T}" />
    /// </summary>
    /// <remarks>
    /// Custom validation extension methods should hang off this type.
    /// </remarks>
    public interface IValidationBuilder<T> : IValidationBuilder
    {
    }
}
