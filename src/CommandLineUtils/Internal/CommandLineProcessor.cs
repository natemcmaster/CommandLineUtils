// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace McMaster.Extensions.CommandLineUtils
{
    internal sealed class CommandLineProcessor
    {
        private readonly CommandLineApplication _app;
        private readonly IReadOnlyList<string> _parameters;
        private readonly CommandLineApplication _initialCommand;
        private readonly ParameterEnumerator _enumerator;
        private CommandLineApplication _currentCommand;
        private CommandArgumentEnumerator _currentCommandArguments;

        public CommandLineProcessor(CommandLineApplication command, IReadOnlyList<string> parameters)
        {
            _app = command;
            _parameters = parameters;
            _initialCommand = command;
            _enumerator = new ParameterEnumerator(parameters);
        }

        public CommandLineApplication Process()
        {
            _currentCommand = _initialCommand;
            _currentCommandArguments = null;
            while (_enumerator.MoveNext())
            {
                if (!ProcessNext())
                {
                    return _currentCommand;
                }
            }
            _enumerator.Reset();
            return _currentCommand;
        }

        private bool ProcessNext()
        {
            switch (_enumerator.Current.Type)
            {
                case ParameterType.ArgumentSeparator:
                    if (!ProcessArgumentSeparator())
                    {
                        return false;
                    }
                    break;
                case ParameterType.ShortOption:
                case ParameterType.LongOption:
                    if (!ProcessOption())
                    {
                        return false;
                    }
                    break;
                case ParameterType.ResponseFile:
                    if (!ProcessResponseFile())
                    {
                        return false;
                    }
                    break;
                case ParameterType.CommandOrArgument:
                    if (!ProcessCommandOrArgument())
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

        private bool ProcessResponseFile()
        {
            if (_currentCommand.ResponseFileHandling == ResponseFileHandling.Disabled)
            {
                return ProcessCommandOrArgument();
            }

            var arg = _enumerator.Current.Raw;
            var path = arg.Substring(1);
            var fullPath = Path.IsPathRooted(path)
                ? path
                : Path.Combine(_currentCommand.WorkingDirectory, path);

            try
            {
                var rspParams = ResponseFileParser.Parse(fullPath, _currentCommand.ResponseFileHandling);
                _enumerator.InsertFromResponseFile(rspParams);
                return true;
            }
            catch (Exception ex)
            {
                throw new CommandParsingException(_currentCommand, $"Could not parse the response file '{arg}'", ex);
            }
        }

        private bool ProcessCommandOrArgument()
        {
            var arg = _enumerator.Current;
            foreach (var subcommand in _currentCommand.Commands)
            {
                if (string.Equals(subcommand.Name, arg.Raw, StringComparison.OrdinalIgnoreCase))
                {
                    _currentCommand = subcommand;
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

        private bool ProcessOption()
        {
            CommandOption option;
            var arg = _enumerator.Current;
            if (arg.Type == ParameterType.ShortOption)
            {
                option = _currentCommand.GetOptions().SingleOrDefault(o => string.Equals(arg.Name, o.ShortName, StringComparison.Ordinal));

                if (option == null)
                {
                    option = _currentCommand.GetOptions().SingleOrDefault(o => string.Equals(arg.Name, o.SymbolName, StringComparison.Ordinal));
                }
            }
            else
            {
                option = _currentCommand.GetOptions().SingleOrDefault(o => string.Equals(arg.Name, o.LongName, StringComparison.Ordinal));
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
                var parent = _currentCommand;
                while (parent.Parent != null) parent = parent.Parent;
                parent.SelectedCommand = _currentCommand;
                if (_currentCommand.StopParsingAfterHelpOption)
                {
                    return false;
                }
            }
            else if (_currentCommand.OptionVersion == option)
            {
                _currentCommand.ShowVersion();
                option.TryParse(null);
                var parent = _currentCommand;
                while (parent.Parent != null) parent = parent.Parent;
                parent.SelectedCommand = _currentCommand;
                if (_currentCommand.StopParsingAfterVersionOption)
                {
                    return false;
                }
            }

            if (arg.Value != null)
            {
                if (!option.TryParse(arg.Value))
                {
                    _currentCommand.ShowHint();
                    throw new CommandParsingException(_currentCommand, $"Unexpected value '{arg.Value}' for option '{arg.Name}'");
                }
            }
            else if (option.OptionType == CommandOptionType.NoValue)
            {
                // No value is needed for this option
                option.TryParse(null);
            }
            else
            {
                if (!_enumerator.MoveNext())
                {
                    _currentCommand.ShowHint();
                    throw new CommandParsingException(_currentCommand, $"Missing value for option '{arg.Name}'");
                }

                if (_enumerator.Current.Type == ParameterType.ResponseFile
                    && _currentCommand.ResponseFileHandling != ResponseFileHandling.Disabled)
                {
                    if (!ProcessResponseFile())
                    {
                        return false;
                    }

                    if (!_enumerator.MoveNext())
                    {
                        _currentCommand.ShowHint();
                        throw new CommandParsingException(_currentCommand, $"Missing value for option '{arg.Name}'");
                    }
                }

                var nextArg = _enumerator.Current;
                if (!option.TryParse(nextArg.Raw))
                {
                    _currentCommand.ShowHint();
                    throw new CommandParsingException(_currentCommand, $"Unexpected value '{nextArg.Raw}' for option '{arg.Name}'");
                }
            }

            return true;
        }

        private bool ProcessArgumentSeparator()
        {
            if (!_currentCommand.AllowArgumentSeparator)
            {
                HandleUnexpectedArg("option");
            }

            if (_enumerator.MoveNext())
            {
                AddRemainingArgumentValues();
            }
            return false;
        }

        private void HandleUnexpectedArg(string argTypeName)
        {
            if (_currentCommand.ThrowOnUnexpectedArgument)
            {
                _currentCommand.ShowHint();
                throw new CommandParsingException(_currentCommand, $"Unrecognized {argTypeName} '{_enumerator.Current.Raw}'");
            }
            else
            {
                // All remaining arguments are stored for further use
                AddRemainingArgumentValues();
            }
        }

        private void AddRemainingArgumentValues()
        {
            do
            {
                _currentCommand.RemainingArguments.Add(_enumerator.Current.Raw);
            } while (_enumerator.MoveNext());
        }

        private enum ParameterType
        {
            CommandOrArgument,
            ShortOption,
            LongOption,
            ResponseFile,
            ArgumentSeparator,
        }

        [DebuggerDisplay("{Raw} ({Type})")]
        private sealed class Parameter
        {
            public Parameter(string raw, bool fromRspFile)
            {
                Raw = raw;
                Type = GetType(raw, fromRspFile);

                if (Type == ParameterType.LongOption || Type == ParameterType.ShortOption)
                {
                    var parts = Raw.Split(new[] { ':', '=' }, 2);
                    if (parts.Length > 1)
                    {
                        Value = parts[1];
                    }

                    var sublen = Type == ParameterType.ShortOption
                        ? 1
                        : 2;
                    Name = parts[0].Substring(sublen);
                }
            }

            public string Raw { get; }
            public string Name { get; }
            public string Value { get; }
            public ParameterType Type { get; }

            private static ParameterType GetType(string raw, bool fromRspFile)
            {
                if (string.IsNullOrEmpty(raw) || raw == "-")
                {
                    return ParameterType.CommandOrArgument;
                }
                else if (raw[0] == '@' && !fromRspFile)
                {
                    // anything that starts with '@' might be a response file
                    // as long as we are not already parsing from a response file
                    return ParameterType.ResponseFile;
                }
                else if (raw[0] != '-')
                {
                    // everything else that does not start with -
                    return ParameterType.CommandOrArgument;
                }
                else if (raw[1] != '-')
                {
                    return ParameterType.ShortOption;
                }
                else if (raw.Length == 2)
                {
                    return ParameterType.ArgumentSeparator;
                }

                return ParameterType.LongOption;
            }
        }

        private sealed class ParameterEnumerator : IEnumerator<Parameter>
        {
            private readonly IEnumerator<string> _enumerator;
            private Parameter _current;
            private IEnumerator<string> _rspEnumerator;

            public ParameterEnumerator(IReadOnlyList<string> parameters)
            {
                _enumerator = parameters.GetEnumerator();
            }

            public Parameter Current => _current;

            object IEnumerator.Current => _current;

            public void InsertFromResponseFile(IEnumerable<string> rspParams)
            {
                _rspEnumerator = rspParams.GetEnumerator();
            }

            public bool MoveNext()
            {
                if (_rspEnumerator != null)
                {
                    if (_rspEnumerator.MoveNext())
                    {
                        _current = new Parameter(_rspEnumerator.Current, fromRspFile: true);
                        return true;
                    }
                    else
                    {
                        _rspEnumerator = null;
                    }
                }

                if (_enumerator.MoveNext())
                {
                    _current = new Parameter(_enumerator.Current, fromRspFile: false);
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _current = null;
                _rspEnumerator = null;
                _enumerator.Reset();
            }

            public void Dispose()
            {
                _current = null;
                _rspEnumerator = null;
                _enumerator.Dispose();
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
