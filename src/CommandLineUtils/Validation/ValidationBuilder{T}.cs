// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// Default implementation of <see cref="IOptionValidationBuilder{T}"/> and <see cref="IArgumentValidationBuilder{T}"/>.
    /// </summary>
    public class ValidationBuilder<T> : ValidationBuilder, IArgumentValidationBuilder<T>, IOptionValidationBuilder<T>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ValidationBuilder"/> for a given <see cref="CommandArgument{T}"/>.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public ValidationBuilder(CommandArgument<T> argument) : base(argument)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ValidationBuilder"/> for a given <see cref="CommandOption{T}"/>.
        /// </summary>
        /// <param name="option">The option.</param>
        public ValidationBuilder(CommandOption<T> option) : base(option)
        {
        }
    }
}
