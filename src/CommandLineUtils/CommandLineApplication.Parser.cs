// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils
{
    partial class CommandLineApplication
    {
        private int Execute(List<string> args)
        {
            var processor = new CommandLineProcessor(this, args);
            var command = processor.Process();

            var result = command.GetValidationResult();

            if (result != ValidationResult.Success)
            {
                return command.ValidationErrorHandler(result);
            }

            return command.Invoke();
        }

        internal CommandLineApplication SelectedCommand { get; set; }

        private bool _settingConsole;

        internal void SetConsole(IConsole console)
        {
            if (_settingConsole)
            {
                // prevent stack overflow in the event someone has looping command line apps
                return;
            }

            _settingConsole = true;
            _console = console;
            Out = console.Out;
            Error = console.Error;

            foreach (var cmd in Commands)
            {
                cmd.SetConsole(console);
            }

            _settingConsole = false;
        }
    }
}
