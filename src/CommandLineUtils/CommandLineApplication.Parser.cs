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
    partial class CommandLineApplication
    {
        // used to keep track of arguments added from the response file
        private int _responseFileArgsEnd = -1;

        private int Execute(List<string> args)
        {
            CommandLineApplication command = this;
            CommandOption option = null;
            IEnumerator<CommandArgument> arguments = null;

            for (var index = 0; index < args.Count; index++)
            {
                var arg = args[index];

                if (command.ResponseFileHandling != ResponseFileHandling.Disabled && index > _responseFileArgsEnd && arg.Length > 1 && arg[0] == '@')
                {
                    var path = arg.Substring(1);
                    var fullPath = Path.IsPathRooted(path)
                        ? path
                        : Path.Combine(command.WorkingDirectory, path);

                    IList<string> rspArgs;
                    try
                    {
                        rspArgs = ResponseFileParser.Parse(fullPath, command.ResponseFileHandling);
                    }
                    catch (Exception ex)
                    {
                        throw new CommandParsingException(this, $"Could not parse the response file '{arg}'", ex);
                    }

                    args.InsertRange(index + 1, rspArgs);
                    _responseFileArgsEnd = index + rspArgs.Count;
                    continue;
                }

                var processed = false;
                if (!processed && option == null)
                {
                    string[] longOption = null;
                    string[] shortOption = null;

                    if (arg != null)
                    {
                        if (arg.StartsWith("--"))
                        {
                            longOption = arg.Substring(2).Split(new[] { ':', '=' }, 2);
                        }
                        else if (arg.StartsWith("-"))
                        {
                            shortOption = arg.Substring(1).Split(new[] { ':', '=' }, 2);
                        }
                    }

                    if (longOption != null)
                    {
                        processed = true;
                        var longOptionName = longOption[0];
                        option = command.GetOptions().SingleOrDefault(opt => string.Equals(opt.LongName, longOptionName, StringComparison.Ordinal));

                        if (option == null)
                        {
                            if (string.IsNullOrEmpty(longOptionName) && !command.ThrowOnUnexpectedArgument && AllowArgumentSeparator)
                            {
                                // skip over the '--' argument separator
                                index++;
                            }

                            HandleUnexpectedArg(command, args, index, argTypeName: "option");
                            break;
                        }

                        // If we find a help/version option, show information and stop parsing
                        if (command.OptionHelp == option)
                        {
                            command.ShowHelp();
                            option.TryParse(null);
                            var parent = command;
                            while (parent.Parent != null) parent = parent.Parent;
                            parent.SelectedCommand = command;
                            if (StopParsingAfterHelpOption)
                            {
                                return 0;
                            }
                        }
                        else if (command.OptionVersion == option)
                        {
                            command.ShowVersion();
                            option.TryParse(null);
                            var parent = command;
                            while (parent.Parent != null) parent = parent.Parent;
                            parent.SelectedCommand = command;
                            if (StopParsingAfterVersionOption)
                            {
                                return 0;
                            }
                        }

                        if (longOption.Length == 2)
                        {
                            if (!option.TryParse(longOption[1]))
                            {
                                command.ShowHint();
                                throw new CommandParsingException(command, $"Unexpected value '{longOption[1]}' for option '{option.LongName}'");
                            }
                            option = null;
                        }
                        else if (option.OptionType == CommandOptionType.NoValue)
                        {
                            // No value is needed for this option
                            option.TryParse(null);
                            option = null;
                        }
                    }

                    if (shortOption != null)
                    {
                        processed = true;
                        option = command.GetOptions().SingleOrDefault(opt => string.Equals(opt.ShortName, shortOption[0], StringComparison.Ordinal));

                        // If not a short option, try symbol option
                        if (option == null)
                        {
                            option = command.GetOptions().SingleOrDefault(opt => string.Equals(opt.SymbolName, shortOption[0], StringComparison.Ordinal));
                        }

                        if (option == null)
                        {
                            HandleUnexpectedArg(command, args, index, argTypeName: "option");
                            break;
                        }

                        // If we find a help/version option, show information and stop parsing
                        if (command.OptionHelp == option)
                        {
                            command.ShowHelp();
                            option.TryParse(null);
                            var parent = command;
                            while (parent.Parent != null) parent = parent.Parent;
                            parent.SelectedCommand = command;
                            if (StopParsingAfterHelpOption)
                            {
                                return 0;
                            }
                        }
                        else if (command.OptionVersion == option)
                        {
                            command.ShowVersion();
                            option.TryParse(null);
                            var parent = command;
                            while (parent.Parent != null) parent = parent.Parent;
                            parent.SelectedCommand = command;
                            if (StopParsingAfterVersionOption)
                            {
                                return 0;
                            }
                        }

                        if (shortOption.Length == 2)
                        {
                            if (!option.TryParse(shortOption[1]))
                            {
                                command.ShowHint();
                                throw new CommandParsingException(command, $"Unexpected value '{shortOption[1]}' for option '{option.LongName}'");
                            }
                            option = null;
                        }
                        else if (option.OptionType == CommandOptionType.NoValue)
                        {
                            // No value is needed for this option
                            option.TryParse(null);
                            option = null;
                        }
                    }
                }

                if (!processed && option != null)
                {
                    processed = true;
                    if (!option.TryParse(arg))
                    {
                        command.ShowHint();
                        throw new CommandParsingException(command, $"Unexpected value '{arg}' for option '{option.LongName}'");
                    }
                    option = null;
                }

                if (!processed && arguments == null)
                {
                    var currentCommand = command;
                    foreach (var subcommand in command.Commands)
                    {
                        if (string.Equals(subcommand.Name, arg, StringComparison.OrdinalIgnoreCase))
                        {
                            processed = true;
                            command = subcommand;
                            break;
                        }
                    }

                    // If we detect a subcommand
                    if (command != currentCommand)
                    {
                        processed = true;
                    }
                }
                if (!processed)
                {
                    if (arguments == null)
                    {
                        arguments = new CommandArgumentEnumerator(command.Arguments.GetEnumerator());
                    }
                    if (arguments.MoveNext())
                    {
                        processed = true;
                        arguments.Current.Values.Add(arg);
                    }
                }
                if (!processed)
                {
                    HandleUnexpectedArg(command, args, index, argTypeName: "command or argument");
                    break;
                }
            }

            if (option != null)
            {
                command.ShowHint();
                throw new CommandParsingException(command, $"Missing value for option '{option.LongName}'");
            }

            var result = command.GetValidationResult();

            if (result != ValidationResult.Success)
            {
                return command.ValidationErrorHandler(result);
            }

            return command.Invoke();
        }

        internal CommandLineApplication SelectedCommand { get; private set; }

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

        private void HandleUnexpectedArg(CommandLineApplication command, IReadOnlyList<string> args, int index, string argTypeName)
        {
            if (command.ThrowOnUnexpectedArgument)
            {
                command.ShowHint();
                throw new CommandParsingException(command, $"Unrecognized {argTypeName} '{args[index]}'");
            }
            else
            {
                // All remaining arguments are stored for further use
                command.RemainingArguments.AddRange(new ArraySegment<string>(args.ToArray(), index, args.Count - index));
            }
        }

        private class CommandArgumentEnumerator : IEnumerator<CommandArgument>
        {
            private readonly IEnumerator<CommandArgument> _enumerator;

            public CommandArgumentEnumerator(IEnumerator<CommandArgument> enumerator)
            {
                _enumerator = enumerator;
            }

            public CommandArgument Current => _enumerator.Current;

            object IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();

            public bool MoveNext()
            {
                if (Current == null || !Current.MultipleValues)
                {
                    return _enumerator.MoveNext();
                }

                // If current argument allows multiple values, we don't move forward and
                // all later values will be added to current CommandArgument.Values
                return true;
            }

            public void Reset() => _enumerator.Reset();
        }
    }
}
