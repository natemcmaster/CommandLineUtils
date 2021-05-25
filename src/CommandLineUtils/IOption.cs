// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Option of the application
    /// </summary>
    public interface IOption
    {
        /// <summary>
        /// Any values found during parsing, if any.
        /// </summary>
        IReadOnlyList<string?> Values { get; }

        /*
        /// <summary>
        /// The default value of the option.
        /// </summary>
        public string? DefaultValue { get; set; }
        */

        /// <summary>
        /// Defines the type of the option.
        /// </summary>
        CommandOptionType OptionType { get; }

        /// <summary>
        /// Determines if subcommands added to <see cref="CommandLineApplication.Commands"/>
        /// should also have access to this option.
        /// </summary>
        bool Inherited { get; set; }

        /// <summary>
        /// A collection of validators that execute before invoking <see cref="CommandLineApplication.OnExecute(Func{int})"/>.
        /// When validation fails, <see cref="CommandLineApplication.ValidationErrorHandler"/> is invoked.
        /// </summary>
        ICollection<IOptionValidator> Validators { get; }

        /// <summary>
        /// A description of this option to show in generated help text and validation.
        /// </summary>
        string? Description { get; set; }

        /// <summary>
        /// Attempt to parse the value that follows after the flag.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryParse(string? value);

        /// <summary>
        /// True when <see cref="Values"/> is not empty. This can also mean that this option has a default value.
        /// </summary>
        /// <returns></returns>
        bool HasValue();

        /// <summary>
        /// Returns the first element of <see cref="Values"/>, if any. This could also be a default-value.
        /// </summary>
        /// <returns></returns>
        string? Value();

        /// <summary>
        /// Clear any parsed values from this argument.
        /// </summary>
        void Reset();
    }
}
