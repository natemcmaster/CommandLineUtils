// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils.Internal.Validators
{
    internal static class InternalValidationExtensions
    {
        public static ValidatorChain True(this ValidatorChainBuilder builder)
            => builder.Build(BooleanValidator.True);

        public static ValidatorChain False(this ValidatorChainBuilder builder)
          => builder.Build(BooleanValidator.False);

        public static ValidatorChain Throw(this ValidatorChainBuilder builder)
            => builder.Build(ThrowingValidator.Singleton);
    }
}
