// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Settings which control the interactive execution behavior.
    /// </summary>
    public class InteractiveExecuteSettings
    {
        /// <summary>
        /// Text used as prompt
        /// </summary>
        public string PromptText { get; set; } = ">";

        /// <summary>
        /// Prompt text color
        /// </summary>
        public ConsoleColor? PromptColor { get; set; }

        /// <summary>
        /// Prompt background color
        /// </summary>
        public ConsoleColor? PromptBgColor { get; set; }

        /// <summary>
        /// Quit or continue interactive session if exception occur on executing command
        /// </summary>
        public bool EndSessionOnException { get; set; } = false;

        /// <summary>
        /// In case the an exception occur while executing a command, it should be rethrow or not
        /// </summary>
        public bool ReThrowException { get; set; } = false;

        /// <summary>
        /// Error message text color
        /// </summary>
        public ConsoleColor? ErrorColor { get; set; }

        /// <summary>
        /// Error background color
        /// </summary>
        public ConsoleColor? ErrorBgColor { get; set; }

        /// <summary>
        /// Action to execute when the inetractive session start
        /// </summary>
        public Action<IConsole> WelcomeAction { get; set; } = null;

        /// <summary>
        /// If not welcome action define, this is the message to show when the interactive session start
        /// </summary>
        public string WelcomeMessage { get; set; }

        /// <summary>
        /// Text color used when shoe the welcome message
        /// </summary>
        public ConsoleColor? WelcomeMessageColor { get; set; }

        /// <summary>
        /// BAckground color used when shoe the welcome message
        /// </summary>
        public ConsoleColor? WelcomeMessageBgColor { get; set; }

        /// <summary>
        /// Define to use or not to use the default quit command to end the interative session
        /// </summary>
        public bool UseDefaultQuitCommand { get; set; } = true;

        /// <summary>
        /// Default quit command to end the interative session
        /// </summary>
        public Action<CommandLineApplication> DefaultQuitCommand { get; set; } = (app) => app.IsInteractive = false;

        /// <summary>
        /// Default quit command name to end the interative session
        /// </summary>
        public string DefaultQuitCommandName { get; set; } = "quit";
    }
}
