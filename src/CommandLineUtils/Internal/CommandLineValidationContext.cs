// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class CommandLineValidationContextFactory
    {
        private readonly CommandLineApplication _app;

        public CommandLineValidationContextFactory(CommandLineApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026",
            Justification = "ValidationContext's constructor requires unreferenced code because the instance's " +
                "type cannot be statically discovered in general. This library never passes the resulting " +
                "context to Validator.TryValidateObject/ValidateObject (which is what would reflect over the " +
                "instance's properties); it only reads ValidationContext.DisplayName/MemberName, which do not " +
                "require reflection over the instance's type.")]
        public ValidationContext Create(CommandLineApplication app) => new(app, _app, null);

        [UnconditionalSuppressMessage("Trimming", "IL2026",
            Justification = "See CommandLineValidationContextFactory.Create(CommandLineApplication).")]
        public ValidationContext Create(CommandArgument argument) => new(argument, _app, null);

        [UnconditionalSuppressMessage("Trimming", "IL2026",
            Justification = "See CommandLineValidationContextFactory.Create(CommandLineApplication).")]
        public ValidationContext Create(CommandOption option) => new(option, _app, null);
    }
}
