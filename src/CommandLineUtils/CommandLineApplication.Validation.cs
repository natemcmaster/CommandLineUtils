// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils
{
    partial class CommandLineApplication : IServiceProvider
    {
        private Func<ValidationResult, int> _validationErrorHandler;

        /// <summary>
        /// The action to call when the command executes, but there was an error validation options or arguments.
        /// The action can return a new validation result.
        /// </summary>
        public Func<ValidationResult, int> ValidationErrorHandler
        {
            get => _validationErrorHandler;
            set => _validationErrorHandler = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Validates arguments and options.
        /// </summary>
        /// <returns>The first validation result that is not <see cref="ValidationResult.Success"/> if there is an error.</returns>
        internal ValidationResult GetValidationResult()
        {
            foreach (var argument in Arguments)
            {
                var context = new ValidationContext(argument, this, null);

                if (!string.IsNullOrEmpty(argument.Name))
                {
                    context.DisplayName = argument.Name;
                    context.MemberName = argument.Name;
                }

                foreach (var validator in argument.Validators)
                {
                    var result = validator.GetValidationResult(argument, context);
                    if (result != ValidationResult.Success)
                    {
                        return result;
                    }
                }
            }

            foreach (var option in GetOptions())
            {
                var context = new ValidationContext(option, this, null);

                string name = null;
                if (option.LongName != null)
                {
                    name = "--" + option.LongName;
                }

                if (name == null && option.ShortName != null)
                {
                    name = "-" + option.ShortName;
                }

                if (name == null && option.SymbolName != null)
                {
                    name = "-" + option.SymbolName;
                }

                if (!string.IsNullOrEmpty(name))
                {
                    context.DisplayName = name;
                    context.MemberName = name;
                }

                foreach (var validator in option.Validators)
                {
                    var result = validator.GetValidationResult(option, context);
                    if (result != ValidationResult.Success)
                    {
                        return result;
                    }
                }
            }

            return ValidationResult.Success;
        }

        private int DefaultValidationErrorHandler(ValidationResult result)
        {
            _console.ForegroundColor = ConsoleColor.Red;
            _console.Error.WriteLine(result.ErrorMessage);
            _console.ResetColor();
            ShowHint();
            return 1;
        }

        /// <summary>
        /// Returns a service provider for some commonly used types associated with a CommandLineApplication.
        /// </summary>
        /// <param name="serviceType">The service type</param>
        /// <returns>An instance of <paramref name="serviceType"/>, or <c>null</c> if it is not available.</returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(CommandLineApplication))
            {
                return this;
            }

            if (serviceType == typeof(IConsole))
            {
                return _console;
            }

            if (State != null && serviceType == State.GetType())
            {
                return State;
            }

            if (serviceType == typeof(IEnumerable<CommandOption>))
            {
                return GetOptions();
            }

            if (serviceType == typeof(IEnumerable<CommandArgument>))
            {
                return Arguments;
            }

            return null;
        }
    }
}
