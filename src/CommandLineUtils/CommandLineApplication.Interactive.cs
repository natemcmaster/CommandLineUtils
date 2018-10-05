// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Internal;
using McMaster.Extensions.CommandLineUtils.IO;
using System;
using System.IO;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Add interactive session capabilities to the <see cref="CommandLineApplication"/>
    /// </summary>
    partial class CommandLineApplication
    {

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="prompt">The prompt implementation to use.</param>
        /// <param name="settings">The settings to use.</param>
        /// <param name="console">The console implementation to use.</param>
        public CommandLineApplication(IPrompt prompt, InteractiveExecuteSettings settings = null, IConsole console = null)
            : this(console: console??PhysicalConsole.Singleton)
        {
            Prompt = prompt;
            InteractiveSettings = settings ?? new InteractiveExecuteSettings();
        }

        /// <summary>
        /// Prompt implementation used in the interactive session.
        /// </summary>
        public IPrompt Prompt { get; private set; } = PhysicalPrompt.Singleton;

        /// <summary>
        /// Settings to control the behavior of the execution.
        /// </summary>
        public InteractiveExecuteSettings InteractiveSettings { get; private set; } = new InteractiveExecuteSettings();

        /// <summary>
        /// Get/Set status of the interactive session. True = Continue collection command, False=Exit
        /// </summary>
        public bool IsInteractive { get; set; } = true;

        /// <summary>
        /// Enter in a loop collecting and executing commands.
        /// </summary>
        /// <returns>The return code from contained CommandLineApplication object's method Execute.</returns>
        public int ExecuteInteractive()
        {
            if(InteractiveSettings.WelcomeAction != null || !String.IsNullOrEmpty(InteractiveSettings.WelcomeMessage))
            {
                ShowWelcomeMessage();
            }

            if(InteractiveSettings.UseDefaultQuitCommand)
            {
                if (!Commands.Exists(a => a.Name == InteractiveSettings.DefaultQuitCommandName))
                {
                    Command(InteractiveSettings.DefaultQuitCommandName, c =>
                    {
                        c.OnExecute(() => IsInteractive = false);
                    });
                }
            }

            var ret = 0;

            do
            {
                var input = Prompt.GetString(InteractiveSettings.PromptText, promptColor: InteractiveSettings.PromptColor, promptBgColor: InteractiveSettings.PromptBgColor);

                var commands = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    ret = Execute(commands);
                }
                catch (Exception ex)
                {
                    if (InteractiveSettings.ReThrowException)
                    {
                        throw ex;
                    }
                    WriteLine(ex.Message, InteractiveSettings.ErrorColor, InteractiveSettings.ErrorBgColor, true);
                    if (InteractiveSettings.EndSessionOnException)
                    {
                        IsInteractive = false;
                    }
                    if(ex is CommandParsingException)
                    {
                        if (ex is UnrecognizedCommandParsingException uex && !string.IsNullOrEmpty(uex.NearestMatch))
                        {
                            WriteLine("", InteractiveSettings.ErrorColor, InteractiveSettings.ErrorBgColor, true);
                            WriteLine("Did you mean this?", InteractiveSettings.ErrorColor, InteractiveSettings.ErrorBgColor, true);
                            WriteLine("    " + uex.NearestMatch, InteractiveSettings.ErrorColor, InteractiveSettings.ErrorBgColor, true);
                        }
                    }
                }
            } while (IsInteractive);

            return ret;
        }

        #region Static executes

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="settings">Settings to use.</param>
        /// <param name="prompt">Prompt to use.</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int ExecuteInteractive<TApp>(CommandLineContext context, InteractiveExecuteSettings settings = null, IPrompt prompt = null)
            where TApp : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            using (var app = new CommandLineApplication<TApp>())
            {
                app.SetContext(context);
                app.Conventions.UseDefaultConventions();
                if (prompt != null)
                {
                    app.Prompt = prompt;
                }
                if (settings != null)
                {
                    app.InteractiveSettings = settings;
                }
                return app.ExecuteInteractive();
            }
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>
        /// </summary>
        /// <param name="settings">The settings to use</param>
        /// <param name="prompt">The prompt to use</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <returns>The process exit code</returns>
        public static int ExecuteInteractive<TApp>(InteractiveExecuteSettings settings = null, IPrompt prompt = null)
            where TApp : class
            => ExecuteInteractive<TApp>(PhysicalConsole.Singleton, settings, prompt);

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>
        /// </summary>
        /// <param name="console">The console to use</param>
        /// <param name="settings">The settings to use</param>
        /// <param name="prompt">The prompt to use</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <returns>The process exit code</returns>
        public static int ExecuteInteractive<TApp>(IConsole console, InteractiveExecuteSettings settings = null, IPrompt prompt = null)
            where TApp : class
        {
            var context = new DefaultCommandLineContext(console, Directory.GetCurrentDirectory(), new string[0]);
            return ExecuteInteractive<TApp>(context, settings, prompt);
        }

        #endregion

        private void ShowWelcomeMessage()
        {
            if(InteractiveSettings.WelcomeAction != null)
            {
                InteractiveSettings.WelcomeAction(_context.Console);
            }
            else if(!String.IsNullOrEmpty(InteractiveSettings.WelcomeMessage))
            {
                WriteLine(InteractiveSettings.WelcomeMessage, InteractiveSettings.WelcomeMessageColor, InteractiveSettings.WelcomeMessageBgColor);
            }
        }

        private void WriteLine(string value, ConsoleColor? foreground, ConsoleColor? background, bool isError = false)
        {
            if (foreground.HasValue)
            {
                _context.Console.ForegroundColor = foreground.Value;
            }

            if (background.HasValue)
            {
                _context.Console.BackgroundColor = background.Value;
            }

            if (!isError)
            {
                _context.Console.WriteLine(value);
            }
            else
            {
                _context.Console.Error.WriteLine(value);
            }

            if (foreground.HasValue || background.HasValue)
            {
                _context.Console.ResetColor();
            }
        }

    }
}
