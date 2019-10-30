// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    internal sealed class CommandLineProcessor
    {
        private readonly CommandLineApplication _initialCommand;
        private readonly ParserConfig _config;
        private readonly ArgumentEnumerator _enumerator;

        private CommandLineApplication _currentCommand
        {
            // this is super hacky and was added to ensure the parser honored quirky behavior in 2.x
            // in which things like response file parsing and working dir could be set per subcommand

            // TODO in 3.0, make parser settings top-level only.
            get => _enumerator.CurrentCommand;
            set => _enumerator.CurrentCommand = value;
        }

        private CommandArgumentEnumerator? _currentCommandArguments;

        public CommandLineProcessor(
            CommandLineApplication command,
            ParserConfig config,
            IReadOnlyList<string> arguments)
        {
            _initialCommand = command;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _enumerator = new ArgumentEnumerator(command, _config, arguments ?? new string[0]);
            CheckForShortOptionClustering(command);
        }

        private static void CheckForShortOptionClustering(CommandLineApplication command)
        {
            if (!command.ClusterOptionsWasSetExplicitly)
            {
                foreach (var option in AllOptions(command))
                {
                    if (option.ShortName != null && option.ShortName.Length != 1)
                    {
                        command.ClusterOptions = false;
                        break;
                    }
                }
            }
            else if (command.ClusterOptions)
            {
                foreach (var option in AllOptions(command))
                {
                    if (option.ShortName != null && option.ShortName.Length != 1)
                    {
                        throw new CommandParsingException(command,
                            $"The ShortName on CommandOption is too long: '{option.ShortName}'. Short names cannot be more than one character long when {nameof(CommandLineApplication.ClusterOptions)} is enabled.");
                    }
                }
            }
        }

        internal static IEnumerable<CommandOption> AllOptions(CommandLineApplication command)
        {
            foreach (var option in command.Options)
            {
                yield return option;
            }

            foreach (var subCommand in command.Commands)
            {
                foreach (var option in AllOptions(subCommand))
                {
                    yield return option;
                }
            }
        }

        public ParseResult Process()
        {
            _currentCommand = _initialCommand;
            _currentCommandArguments = null;
            while (_enumerator.MoveNext())
            {
                if (!ProcessNext())
                {
                    goto finished;
                }
            }

            _enumerator.Reset();

        finished:
            return new ParseResult(_currentCommand);
        }

        private bool ProcessNext()
        {
            switch (_enumerator.Current)
            {
                case ArgumentSeparatorArgument _:
                    if (!ProcessArgumentSeparator())
                    {
                        return false;
                    }

                    break;
                case OptionArgument arg:
                    if (!ProcessOption(arg))
                    {
                        return false;
                    }

                    break;
                case CommandOrParameterArgument arg:
                    if (!ProcessCommandOrParameter(arg))
                    {
                        return false;
                    }

                    break;
                default:
                    HandleUnexpectedArg("command or argument");
                    return false;
            }

            return true;
        }

        private bool ProcessCommandOrParameter(CommandOrParameterArgument arg)
        {
            foreach (var subcommand in _currentCommand.Commands)
            {
                if (subcommand.MatchesName(arg.Raw))
                {
                    _currentCommand = subcommand;
                    // Reset the arguments enumerator when moving down the subcommand tree.
                    _currentCommandArguments = null;
                    return true;
                }
            }

            if (_currentCommandArguments == null)
            {
                _currentCommandArguments = new CommandArgumentEnumerator(_currentCommand.Arguments.GetEnumerator());
            }

            if (_currentCommandArguments.MoveNext())
            {
                _currentCommandArguments.Current.Values.Add(arg.Raw);
            }
            else
            {
                HandleUnexpectedArg("command or argument");
                return false;
            }

            return true;
        }

        private bool ProcessOption(OptionArgument arg)
        {
            CommandOption? option = null;
            var value = arg.Value;
            var name = arg.Name;
            if (arg.IsShortOption)
            {
                if (_currentCommand.ClusterOptions)
                {
                    for (var i = 0; i < arg.Name.Length; i++)
                    {
                        var ch = arg.Name.Substring(i, 1);

                        option = FindOption(ch, o => o.ShortName);

                        if (option == null)
                        {
                            // quirk for compatibility with symbol options
                            option = FindOption(ch, o => o.SymbolName);
                        }

                        if (option == null)
                        {
                            HandleUnexpectedArg("option", "-" + ch);
                            return false;
                        }

                        // If we find a help/version option, show information and stop parsing
                        if (_currentCommand.OptionHelp == option)
                        {
                            _currentCommand.ShowHelp();
                            option.TryParse(null);
                            return false;
                        }

                        if (_currentCommand.OptionVersion == option)
                        {
                            _currentCommand.ShowVersion();
                            option.TryParse(null);
                            return false;
                        }

                        name = ch;

                        var isLastChar = i == arg.Name.Length - 1;
                        if (option.OptionType == CommandOptionType.NoValue)
                        {
                            if (!isLastChar)
                            {
                                option.TryParse(null);
                            }
                        }
                        else if (option.OptionType == CommandOptionType.SingleOrNoValue)
                        {
                            if (!isLastChar)
                            {
                                option.TryParse(null);
                            }
                        }
                        else if (!isLastChar)
                        {
                            if (value != null)
                            {
                                // if an option was also specified using :value or =value at the end of the option
                                _currentCommand.ShowHint();
                                throw new CommandParsingException(_currentCommand, $"Option '{ch}', which requires a value, must be the last option in a cluster");
                            }

                            // supports specifying the value as the last bit of the flag. -Xignore-whitespace
                            value = arg.Name.Substring(i + 1);
                            break;
                        }
                    }
                }
                else
                {
                    option = FindOption(name, o => o.ShortName);

                    if (option == null)
                    {
                        option = FindOption(name, o => o.SymbolName);
                    }
                }
            }
            else
            {
                option = FindOption(name, o => o.LongName);
            }

            if (option == null)
            {
                HandleUnexpectedArg("option");
                return false;
            }

            // If we find a help/version option, show information and stop parsing
            if (_currentCommand.OptionHelp == option)
            {
                _currentCommand.ShowHelp();
                option.TryParse(null);
                return false;
            }

            if (_currentCommand.OptionVersion == option)
            {
                _currentCommand.ShowVersion();
                option.TryParse(null);
                return false;
            }

            if (value != null)
            {
                if (!option.TryParse(value))
                {
                    _currentCommand.ShowHint();
                    throw new CommandParsingException(_currentCommand,
                        $"Unexpected value '{value}' for option '{name}'");
                }
            }
            else if (option.OptionType == CommandOptionType.NoValue
                     || option.OptionType == CommandOptionType.SingleOrNoValue)
            {
                // No value is needed for this option
                option.TryParse(null);
            }
            else
            {
                if (_config.OptionNameAndValueCanBeSpaceSeparated)
                {
                    if (_enumerator.MoveNext())
                    {
                        value = _enumerator.Current?.Raw;
                    }
                    else
                    {
                        _currentCommand.ShowHint();
                        throw new CommandParsingException(_currentCommand, $"Missing value for option '{name}'");
                    }
                }
                else if (value == null)
                {
                    _currentCommand.ShowHint();
                    throw new CommandParsingException(_currentCommand, $"Missing value for option '{name}'");
                }

                if (!option.TryParse(value))
                {
                    _currentCommand.ShowHint();
                    throw new CommandParsingException(_currentCommand,
                        $"Unexpected value '{value}' for option '{name}'");
                }
            }

            return true;
        }

        private CommandOption? FindOption(string name, Func<CommandOption, string?> by)
        {
            var options = _currentCommand
                .GetOptions()
                .Where(o => string.Equals(name, by(o), _currentCommand.OptionsComparison))
                .ToList();

            if (options.Count == 0)
            {
                return null;
            }

            if (options.Count == 1)
            {
                return options.First();
            }

            var helpOption = options.SingleOrDefault(o => o == _currentCommand.OptionHelp);
            if (helpOption != null)
            {
                return helpOption;
            }

            throw new InvalidOperationException($"Multiple options with name \"{name}\" found. This is usually due to nested options.");
        }

        private bool ProcessArgumentSeparator()
        {
            if (!_currentCommand.AllowArgumentSeparator)
            {
                HandleUnexpectedArg("option");
            }

            _enumerator.DisableResponseFileLoading = true;

            if (_enumerator.MoveNext())
            {
                AddRemainingArgumentValues();
            }

            return false;
        }

        private void HandleUnexpectedArg(string argTypeName, string? argValue = null)
        {
            if (_currentCommand.ThrowOnUnexpectedArgument)
            {
                _currentCommand.ShowHint();
                var value = argValue ?? _enumerator.Current?.Raw;

                var suggestions = Enumerable.Empty<string>();

                if (_currentCommand.MakeSuggestionsInErrorMessage && !string.IsNullOrEmpty(value))
                {
                    suggestions = SuggestionCreator.GetTopSuggestions(_currentCommand, value);
                }

                throw new UnrecognizedCommandParsingException(_currentCommand, suggestions,
                    $"Unrecognized {argTypeName} '{value}'");
            }

            // All remaining arguments are stored for further use
            AddRemainingArgumentValues();
        }

        private void AddRemainingArgumentValues()
        {
            do
            {
                var arg = _enumerator.Current;
                if (arg != null)
                {
                    _currentCommand.RemainingArguments.Add(arg.Raw);
                }
            } while (_enumerator.MoveNext());
        }

        private class ArgumentSeparatorArgument : Argument
        {
            private ArgumentSeparatorArgument() : base("--")
            {
            }

            public static ArgumentSeparatorArgument Instance { get; } = new ArgumentSeparatorArgument();
        }

        private class CommandOrParameterArgument : Argument
        {
            public CommandOrParameterArgument(string raw) : base(raw)
            {
            }
        }

        private class OptionArgument : Argument
        {
            public OptionArgument(string raw, char[] nameValueSeparators, bool isShortOption) : base(raw)
            {
                IsShortOption = isShortOption;
                var parts = Raw.Split(nameValueSeparators, 2);
                if (parts.Length > 1)
                {
                    Value = parts[1];
                }

                var sublen = isShortOption
                    ? 1
                    : 2;
                Name = parts[0].Substring(sublen);
            }

            public string Name { get; }
            public string? Value { get; }
            public bool IsShortOption { get; }
        }

        [DebuggerDisplay("{Raw} ({Type})")]
        private abstract class Argument
        {
            public Argument(string raw)
            {
                Raw = raw;
            }

            public string Raw { get; }
        }

        private sealed class ArgumentEnumerator : IEnumerator<Argument?>
        {
            private readonly IEnumerator<string> _rawArgEnumerator;
            private IEnumerator<string>? _rspEnumerator;
            private readonly ParserConfig _config;

            public ArgumentEnumerator(CommandLineApplication command, ParserConfig config, IReadOnlyList<string> rawArguments)
            {
                _config = config;
                CurrentCommand = command;
                _rawArgEnumerator = rawArguments.GetEnumerator();
            }

            public Argument? Current { get; private set; }

            object? IEnumerator.Current => Current;

            // currently this must be settable because some parsing behavior can be set per subcommand
            public CommandLineApplication CurrentCommand { get; set; }

            public bool DisableResponseFileLoading { get; set; }

            public bool MoveNext()
            {
                if (_rspEnumerator != null)
                {
                    if (_rspEnumerator.MoveNext())
                    {
                        Current = CreateArgument(_rspEnumerator.Current);
                        return true;
                    }

                    _rspEnumerator = null;
                }

                if (_rawArgEnumerator.MoveNext())
                {
                    if (CurrentCommand.ResponseFileHandling != ResponseFileHandling.Disabled
                        && !DisableResponseFileLoading)
                    {
                        var raw = _rawArgEnumerator.Current;
                        if (raw != null && raw.Length > 1 && raw[0] == '@')
                        {
                            _rspEnumerator = CreateRspParser(raw.Substring(1));
                            return MoveNext();
                        }
                    }

                    Current = CreateArgument(_rawArgEnumerator.Current);
                    return true;
                }

                return false;
            }

            private Argument CreateArgument(string raw)
            {
                if (string.IsNullOrEmpty(raw) || raw == "-" || raw[0] != '-')
                {
                    return new CommandOrParameterArgument(raw);
                }

                if (raw[1] != '-')
                {
                    return new OptionArgument(raw, _config.OptionNameValueSeparators, isShortOption: true);
                }

                if (raw.Length == 2)
                {
                    return ArgumentSeparatorArgument.Instance;
                }

                return new OptionArgument(raw, _config.OptionNameValueSeparators, isShortOption: false);
            }

            private IEnumerator<string> CreateRspParser(string path)
            {
                var fullPath = Path.IsPathRooted(path)
                    ? path
                    : Path.Combine(CurrentCommand.WorkingDirectory, path);

                try
                {
                    var rspParams = ResponseFileParser.Parse(fullPath, CurrentCommand.ResponseFileHandling);
                    return rspParams.GetEnumerator();
                }
                catch (Exception ex)
                {
                    throw new CommandParsingException(CurrentCommand, $"Could not parse the response file '{path}'", ex);
                }
            }

            public void Reset()
            {
                Current = null;
                _rspEnumerator = null;
                _rawArgEnumerator.Reset();
            }

            public void Dispose()
            {
                Current = null;
                _rspEnumerator = null;
                _rawArgEnumerator.Dispose();
            }
        }

        private sealed class CommandArgumentEnumerator : IEnumerator<CommandArgument>
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
