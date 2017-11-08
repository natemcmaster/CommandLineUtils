// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class ReflectionAppBuilder<T>
        where T : class, new()
    {
        private readonly CommandLineApplication _app;
        private readonly List<Action<T>> _binders = new List<Action<T>>();

        private delegate void SetPropertyDelegate(object obj, object value);

        public ReflectionAppBuilder()
        {
            _app = new CommandLineApplication();

            var type = typeof(T).GetTypeInfo();
            var helpOptionAttrOnType = type.GetCustomAttribute<HelpOptionAttribute>();
            if (helpOptionAttrOnType != null)
            {
                var opt = _app.HelpOption(helpOptionAttrOnType.Template);
                opt.Description = helpOptionAttrOnType.Description;
                opt.Inherited = helpOptionAttrOnType.Inherited;
                opt.ShowInHelpText = helpOptionAttrOnType.ShowInHelpText;
            }

            var props = typeof(T).GetRuntimeProperties();
            if (props != null)
            {
                var argOrder = new SortedList<int, CommandArgument>();
                var argPropOrder = new Dictionary<int, PropertyInfo>();
                var shortOptions = new Dictionary<string, PropertyInfo>();
                var longOptions = new Dictionary<string, PropertyInfo>();

                foreach (var prop in props)
                {
                    OptionAttributeBase optionAttr;
                    var helpOptionAttr = prop.GetCustomAttribute<HelpOptionAttribute>();
                    var regularOptionAttr = prop.GetCustomAttribute<OptionAttribute>();

                    if (helpOptionAttr != null)
                    {
                        if (helpOptionAttrOnType != null)
                        {
                            throw new InvalidOperationException(Strings.HelpOptionOnTypeAndProperty);
                        }

                        if (_app.OptionHelp != null)
                        {
                            throw new InvalidOperationException(Strings.MultipleHelpOptionPropertiesFound);
                        }
                        
                        if (regularOptionAttr != null)
                        {
                            throw new InvalidOperationException(
                                Strings.BothOptionAndHelpOptionAttributesCannotBeSpecified(prop));
                        }

                        optionAttr = helpOptionAttr;
                    }
                    else
                    {
                        optionAttr = regularOptionAttr;
                    }

                    var argumentAttr = prop.GetCustomAttribute<ArgumentAttribute>();
                    var setter = GetSetter(prop);

                    if (optionAttr != null && argumentAttr != null)
                    {
                        throw new InvalidOperationException(
                            Strings.BothOptionAndArgumentAttributesCannotBeSpecified(prop));
                    }

                    if (argumentAttr != null)
                    {
                        if ((prop.PropertyType.IsArray
                             || typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(prop.PropertyType))
                            && prop.PropertyType != typeof(string)
                            && !argumentAttr.MultipleValues)
                        {
                            throw new InvalidOperationException(Strings.MultipleValuesArgumentShouldBeCollection);
                        }

                        var argument = new CommandArgument
                        {
                            Name = argumentAttr.Name ?? prop.Name,
                            Description = argumentAttr.Description,
                            ShowInHelpText = argumentAttr.ShowInHelpText,
                            MultipleValues = argumentAttr.MultipleValues,
                        };

                        if (argPropOrder.TryGetValue(argumentAttr.Order, out var otherProp))
                        {
                            throw new InvalidOperationException(
                                Strings.DuplicateArgumentPosition(argumentAttr.Order, prop, otherProp));
                        }

                        argPropOrder.Add(argumentAttr.Order, prop);
                        argOrder.Add(argumentAttr.Order, argument);

                        if (argument.MultipleValues)
                        {
                            var collectionParser = CollectionParserProvider.Default.GetParser(prop.PropertyType);
                            if (collectionParser == null)
                            {
                                throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                            }

                            OnBind(o => { setter.Invoke(o, collectionParser.Parse(argument.Name, argument.Values)); });
                        }
                        else
                        {
                            var parser = ValueParserProvider.Default.GetParser(prop.PropertyType);
                            if (parser == null)
                            {
                                throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                            }
                            OnBind(o =>
                                setter.Invoke(o, parser.Parse(argument.Name, argument.Value)));
                        }
                    }

                    if (optionAttr != null)
                    {
                        CommandOptionType optionType;
                        if (optionAttr is OptionAttribute oa)
                        {
                            optionType = GetOptionType(oa, prop);
                        }
                        else
                        {
                            optionType = CommandOptionType.NoValue;
                        }
                        
                        if (optionType == CommandOptionType.NoValue && prop.PropertyType != typeof(bool))
                        {
                            throw new InvalidOperationException(Strings.NoValueTypesMustBeBoolean);
                        }

                        CommandOption option;
                        if (optionAttr.Template != null)
                        {
                            option = new CommandOption(optionAttr.Template, optionType);
                        }
                        else
                        {
                            var longName = prop.Name.ToKebabCase();
                            option = new CommandOption(optionType)
                            {
                                LongName = longName,
                                ShortName = longName.Substring(0, 1),
                                ValueName = prop.Name,
                            };
                        }

                        option.Description = optionAttr.Description;
                        option.ShowInHelpText = optionAttr.ShowInHelpText;
                        option.Inherited = optionAttr.Inherited;

                        if (option.ShortName != null)
                        {
                            if (shortOptions.TryGetValue(option.ShortName, out var otherProp))
                            {
                                throw new InvalidOperationException(
                                    Strings.OptionNameIsAmbiguous(option.ShortName, prop, otherProp));
                            }
                            shortOptions.Add(option.ShortName, prop);
                        }

                        if (option.LongName != null)
                        {
                            if (longOptions.TryGetValue(option.LongName, out var otherProp))
                            {
                                throw new InvalidOperationException(
                                    Strings.OptionNameIsAmbiguous(option.LongName, prop, otherProp));
                            }
                            longOptions.Add(option.LongName, prop);
                        }

                        if (optionAttr is HelpOptionAttribute)
                        {
                            _app.OptionHelp = option;
                        }
                        else
                        {
                            _app.Options.Add(option);
                        }

                        switch (option.OptionType)
                        {
                            case CommandOptionType.MultipleValue:
                                var collectionParser = CollectionParserProvider.Default.GetParser(prop.PropertyType);
                                if (collectionParser == null)
                                {
                                    throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                                }
                                OnBind(o =>
                                {
                                    setter.Invoke(o, collectionParser.Parse(option.LongName, option.Values));
                                });
                                break;
                            case CommandOptionType.SingleValue:
                                var parser = ValueParserProvider.Default.GetParser(prop.PropertyType);
                                if (parser == null)
                                {
                                    throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                                }
                                OnBind(o =>
                                {
                                    var value = option.Value();
                                    if (value == null)
                                    {
                                        return;
                                    }
                                    setter.Invoke(o, parser.Parse(option.LongName, value));
                                });
                                break;
                            case CommandOptionType.NoValue:
                                OnBind(o => { setter.Invoke(o, option.HasValue()); });
                                break;
                        }
                    }
                }

                foreach (var arg in argOrder)
                {
                    if (_app.Arguments.Count > 0)
                    {
                        var lastArg = _app.Arguments[_app.Arguments.Count - 1];
                        if (lastArg.MultipleValues)
                        {
                            throw new InvalidOperationException(
                                Strings.OnlyLastArgumentCanAllowMultipleValues(lastArg.Name));
                        }
                    }

                    _app.Arguments.Add(arg.Value);
                }
            }
        }

        private static CommandOptionType GetOptionType(OptionAttribute optionAttr, PropertyInfo prop)
        {
            CommandOptionType optionType;
            if (optionAttr.OptionType.HasValue)
            {
                optionType = optionAttr.OptionType.Value;
            }
            else if (!CommandOptionTypeMapper.Default.TryGetOptionType(prop.PropertyType, out optionType))
            {
                throw new InvalidOperationException(Strings.CannotDetermineOptionType(prop));
            }
            return optionType;
        }

        private static SetPropertyDelegate GetSetter(PropertyInfo prop)
        {
            var setter = prop.GetSetMethod(nonPublic: true);
            if (setter != null)
            {
                return (obj, value) => setter.Invoke(obj, new object[] {value});
            }
            else
            {
                var backingFieldName = string.Format("<{0}>k__BackingField", prop.Name);
                var backingField = prop.DeclaringType.GetTypeInfo().GetDeclaredField(backingFieldName);
                if (backingField == null)
                {
                    throw new InvalidOperationException(
                        $"Could not find a way to set {prop.DeclaringType.FullName}.{prop.Name}");
                }

                return (obj, value) => backingField.SetValue(obj, value);
            }
        }

        public CommandLineApplication App => _app;

        public T Execute(string[] args, CommandParsingOptions parsingOptions)
        {
            if ((parsingOptions & CommandParsingOptions.ThrowOnUnexpectedArgument) != 0)
            {
                _app.ThrowOnUnexpectedArgument = true;
            }

            if ((parsingOptions & CommandParsingOptions.AllowArgumentSeparator) != 0)
            {
                _app.AllowArgumentSeparator = true;
            }

            if ((parsingOptions & CommandParsingOptions.HandleResponseFiles) != 0)
            {
                _app.HandleResponseFiles = true;
            }

            var options = new T();
            _app.Execute(args);

            foreach (var binder in _binders)
            {
                binder(options);
            }

            return options;
        }

        private void OnBind(Action<T> onBind)
        {
            _binders.Add(onBind);
        }
    }
}