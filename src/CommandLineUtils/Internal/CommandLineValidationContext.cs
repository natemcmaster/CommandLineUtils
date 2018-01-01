// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class CommandLineValidationContextFactory
    {
        private readonly CommandLineApplication _app;

        public CommandLineValidationContextFactory(CommandLineApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public ValidationContext Create(CommandArgument argument)
            => new ValidationContext(argument, new ServiceProvider(_app), null);

        public ValidationContext Create(CommandOption option)
            => new ValidationContext(option, new ServiceProvider(_app), null);

        private sealed class ServiceProvider : IServiceProvider
        {
            private readonly CommandLineApplication _parent;

            public ServiceProvider(CommandLineApplication parent)
            {
                _parent = parent;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(CommandLineApplication))
                {
                    return _parent;
                }

                if (serviceType == typeof(IConsole))
                {
                    return _parent._context.Console;
                }

                if (serviceType == typeof(IEnumerable<CommandOption>))
                {
                    return _parent.GetOptions();
                }

                if (serviceType == typeof(IEnumerable<CommandArgument>))
                {
                    return _parent.Arguments;
                }

                if (serviceType == typeof(CommandLineContext))
                {
                    return _parent._context;
                }

                return null;
            }
        }
    }
}
