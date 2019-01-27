// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Provides validation for <see cref="CommandArgument"/>, <see cref="CommandOption"/>, and <see cref="ICommandValidator"/>.
    /// </summary>
    public interface IValidator : IOptionValidator, IArgumentValidator, ICommandValidator
    {
    }
}
