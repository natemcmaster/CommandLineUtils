// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Creates a collection of validators for <see cref="CommandOption{T}"/>.
    /// </summary>
    /// <remarks>
    /// Custom validation extension methods that only apply to <see cref="CommandOption{T}"/> should hang off this type.
    /// </remarks>
    public interface IOptionValidationBuilder<T> : IOptionValidationBuilder, IValidationBuilder<T>
    {
    }
}
