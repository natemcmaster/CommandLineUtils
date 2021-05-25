// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils
{
    partial class CommandLineApplication
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
        /// A collection of validators that execute before invoking <see cref="OnExecute(Func{int})"/>.
        /// When validation fails, <see cref="ValidationErrorHandler"/> is invoked.
        /// </summary>
        public ICollection<ICommandValidator> Validators { get; } = new List<ICommandValidator>();

        /// <summary>
        /// Validates arguments and options.
        /// </summary>
        /// <returns>The first validation result that is not <see cref="ValidationResult.Success"/> if there is an error.</returns>
        public ValidationResult? GetValidationResult()
        {
            if (Parent != null)
            {
                var result = Parent.GetValidationResult();

                if (result != ValidationResult.Success)
                {
                    return result;
                }
            }

            var factory = new CommandLineValidationContextFactory(this);

            var commandContext = factory.Create(this);
            foreach (var validator in Validators)
            {
                var result = validator.GetValidationResult(this, commandContext);
                if (result != ValidationResult.Success)
                {
                    return result;
                }
            }

            foreach (var argument in Arguments)
            {
                var context = factory.Create(argument);

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

            foreach (var option in GetAnyOptions())
            {
                var context = factory.Create(option);

                if (option is IParseableOption parseableOption)
                {
                    string? name = null;
                    if (parseableOption.LongName != null)
                    {
                        name = "--" + parseableOption.LongName;
                    }

                    if (name == null && parseableOption.ShortName != null)
                    {
                        name = "-" + parseableOption.ShortName;
                    }

                    if (name == null && parseableOption.SymbolName != null)
                    {
                        name = "-" + parseableOption.SymbolName;
                    }

                    if (name == null && parseableOption.ValueName != null)
                    {
                        name = parseableOption.ValueName;
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        context.DisplayName = name;
                        context.MemberName = name;
                    }
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
            _context.Console.ForegroundColor = ConsoleColor.Red;
            _context.Console.Error.WriteLine(result.ErrorMessage);
            _context.Console.ResetColor();
            ShowHint();
            return ValidationErrorExitCode;
        }
    }
}
