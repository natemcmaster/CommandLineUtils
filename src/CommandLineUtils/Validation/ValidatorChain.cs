// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using static System.ComponentModel.DataAnnotations.ValidationResult;

namespace McMaster.Extensions.CommandLineUtils.Validation
{
    /// <summary>
    /// A chain of validators.
    /// This is normally created by calling <see cref="ValidatorChainBuilder.Build(IValidator)"/>.
    /// </summary>
    public class ValidatorChain : IValidator
    {
        private readonly IValidator _rule;

        /// <summary>
        /// Creates an instance of <see cref="ValidatorChain"/>.
        /// </summary>
        /// <param name="validator">The validator</param>
        public ValidatorChain(IValidator validator)
        {
            _rule = validator;
        }

        /// <summary>
        /// Add an additional validation condition that must also be true for the parameter value to be considered valid.
        /// </summary>
        public ValidatorChainBuilder And
            => new CompositeValidationBuilder(_rule, CompositeRule.And);

        /// <summary>
        /// Add an alternate condition under which the parameter value can be considered valid.
        /// </summary>
        public ValidatorChainBuilder Or
            => new CompositeValidationBuilder(_rule, CompositeRule.Or);

        ValidationResult IOptionValidator.GetValidationResult(CommandOption option, ValidationContext context)
        {
            return _rule.GetValidationResult(option, context);
        }

        ValidationResult IArgumentValidator.GetValidationResult(CommandArgument argument, ValidationContext context)
        {
            return _rule.GetValidationResult(argument, context);
        }

        private class CompositeValidationBuilder : ValidatorChainBuilder
        {
            private IValidator _rule;
            private CompositeRule _comparison;

            public CompositeValidationBuilder(IValidator rule, CompositeRule comparison)
            {
                _rule = rule;
                _comparison = comparison;
            }

            public override ValidatorChain Build(IValidator rule)
            {
                return new ValidatorChain(new CompositeValidator(_rule, rule, _comparison));
            }
        }

        private enum CompositeRule
        {
            And,
            Or,
        }

        private class CompositeValidator : IValidator
        {
            private readonly IValidator _left;
            private readonly IValidator _right;
            private readonly CompositeRule _comparison;

            public CompositeValidator(IValidator left, IValidator right, CompositeRule comparison)
            {
                _left = left;
                _right = right;
                _comparison = comparison;
            }

            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
                => GetResult(
                    () => _left.GetValidationResult(option, context),
                    () => _right.GetValidationResult(option, context));

            public ValidationResult GetValidationResult(CommandArgument argument, ValidationContext context)
                => GetResult(
                  () => _left.GetValidationResult(argument, context),
                  () => _right.GetValidationResult(argument, context));

            private ValidationResult GetResult(Func<ValidationResult> leftGetter, Func<ValidationResult> rightGetter)
            {
                var leftResult = leftGetter();
                switch (_comparison)
                {
                    case CompositeRule.And:
                        if (leftResult != Success)
                        {
                            // TODO show error message for both conditions.
                            return leftResult;
                        }

                        var rightResult = rightGetter();
                        if (rightResult != Success)
                        {
                            return rightResult;
                        }

                        return Success;

                    case CompositeRule.Or:
                        if (leftResult == Success)
                        {
                            return Success;
                        }

                        rightResult = rightGetter();
                        if (rightResult == Success)
                        {
                            return Success;
                        }

                        // TODO: Combine both error messages
                        return leftResult;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
