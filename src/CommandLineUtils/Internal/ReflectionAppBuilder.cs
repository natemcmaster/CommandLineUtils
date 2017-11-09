// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Creates a <see cref="CommandLineApplication"/> as defined by attributes.
    /// </summary>
    internal class ReflectionAppBuilder
    {
        private readonly List<Action<object>> _binders = new List<Action<object>>();
        private readonly HashSet<Type> _addedTypes = new HashSet<Type>();
        private readonly SortedList<int, CommandArgument> _argOrder = new SortedList<int, CommandArgument>();
        private readonly Dictionary<int, PropertyInfo> _argPropOrder = new Dictionary<int, PropertyInfo>();
        private readonly Dictionary<string, PropertyInfo> _shortOptions = new Dictionary<string, PropertyInfo>();
        private readonly Dictionary<string, PropertyInfo> _longOptions = new Dictionary<string, PropertyInfo>();

        private delegate void SetPropertyDelegate(object obj, object value);

        public CommandLineApplication App { get; } = new CommandLineApplication();

        public T Execute<T>(string[] args, CommandParsingOptions parsingOptions)
            where T : class, new()
        {
            AddType<T>();
            
            if ((parsingOptions & CommandParsingOptions.ThrowOnUnexpectedArgument) != 0)
            {
                App.ThrowOnUnexpectedArgument = true;
            }

            if ((parsingOptions & CommandParsingOptions.AllowArgumentSeparator) != 0)
            {
                App.AllowArgumentSeparator = true;
            }

            if ((parsingOptions & CommandParsingOptions.HandleResponseFiles) != 0)
            {
                App.HandleResponseFiles = true;
            }

            var options = new T();
            App.Execute(args);

            foreach (var binder in _binders)
            {
                binder(options);
            }

            return options;
        }

        public void AddType<T>()
            where T : class, new()
        {
            if (_addedTypes.Contains(typeof(T)))
            {
                return;
            }
            
            _addedTypes.Add(typeof(T));

            var typeInfo = typeof(T).GetTypeInfo();
            var helpOptionAttrOnType = typeInfo.GetCustomAttribute<HelpOptionAttribute>();
            if (helpOptionAttrOnType != null)
            {
                var opt = App.HelpOption(helpOptionAttrOnType.Template);
                opt.Description = helpOptionAttrOnType.Description;
                opt.Inherited = helpOptionAttrOnType.Inherited;
                opt.ShowInHelpText = helpOptionAttrOnType.ShowInHelpText;
            }

            var versionOptionAttrOnType = typeInfo.GetCustomAttribute<VersionOptionAttribute>();
            if (versionOptionAttrOnType != null)
            {
                var opt = App.VersionOption(versionOptionAttrOnType.Template, versionOptionAttrOnType.Version);
                opt.Description = versionOptionAttrOnType.Description;
                opt.Inherited = versionOptionAttrOnType.Inherited;
                opt.ShowInHelpText = versionOptionAttrOnType.ShowInHelpText;
            }

            var props = typeof(T).GetRuntimeProperties();
            if (props != null)
            {
                AddProperties(props, helpOptionAttrOnType != null, versionOptionAttrOnType != null);
            }
        }

        private void AddProperties(IEnumerable<PropertyInfo> props, 
            bool hasHelpOptionAttrOnType,
            bool hasVersionOptionAttrOnType)
        {
            foreach (var prop in props)
            {
                OptionAttributeBase optionAttr;
                var helpOptionAttr = prop.GetCustomAttribute<HelpOptionAttribute>();
                var versionOptionAttr = prop.GetCustomAttribute<VersionOptionAttribute>();
                var regularOptionAttr = prop.GetCustomAttribute<OptionAttribute>();

                if (helpOptionAttr != null && versionOptionAttr != null)
                {
                    throw new InvalidOperationException(
                        Strings.BothHelpOptionAndVersionOptionAttributesCannotBeSpecified(prop));
                }
                
                if (helpOptionAttr != null)
                {
                    if (hasHelpOptionAttrOnType)
                    {
                        throw new InvalidOperationException(Strings.HelpOptionOnTypeAndProperty);
                    }

                    if (App.OptionHelp != null)
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
                else if (versionOptionAttr != null)
                {
                    if (hasVersionOptionAttrOnType)
                    {
                        throw new InvalidOperationException(Strings.VersionOptionOnTypeAndProperty);
                    }

                    if (App.OptionVersion != null)
                    {
                        throw new InvalidOperationException(Strings.MultipleVersionOptionPropertiesFound);
                    }

                    if (regularOptionAttr != null)
                    {
                        throw new InvalidOperationException(
                            Strings.BothOptionAndVersionOptionAttributesCannotBeSpecified(prop));
                    }

                    optionAttr = versionOptionAttr;
                }
                else
                {
                    optionAttr = regularOptionAttr;
                }

                var argumentAttr = prop.GetCustomAttribute<ArgumentAttribute>();

                if (optionAttr != null && argumentAttr != null)
                {
                    throw new InvalidOperationException(
                        Strings.BothOptionAndArgumentAttributesCannotBeSpecified(prop));
                }

                if (argumentAttr != null)
                {
                    AddArgument(prop, argumentAttr);
                }

                if (optionAttr != null)
                {
                    AddOption(optionAttr, prop);
                }
            }

            // in the event AddType gets called multiple times
            App.Arguments.Clear();
            
            foreach (var arg in _argOrder)
            {
                if (App.Arguments.Count > 0)
                {
                    var lastArg = App.Arguments[App.Arguments.Count - 1];
                    if (lastArg.MultipleValues)
                    {
                        throw new InvalidOperationException(
                            Strings.OnlyLastArgumentCanAllowMultipleValues(lastArg.Name));
                    }
                }

                App.Arguments.Add(arg.Value);
            }
        }

        private void AddOption(OptionAttributeBase optionAttr, PropertyInfo prop)
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
                if (_shortOptions.TryGetValue(option.ShortName, out var otherProp))
                {
                    throw new InvalidOperationException(
                        Strings.OptionNameIsAmbiguous(option.ShortName, prop, otherProp));
                }
                _shortOptions.Add(option.ShortName, prop);
            }

            if (option.LongName != null)
            {
                if (_longOptions.TryGetValue(option.LongName, out var otherProp))
                {
                    throw new InvalidOperationException(
                        Strings.OptionNameIsAmbiguous(option.LongName, prop, otherProp));
                }
                _longOptions.Add(option.LongName, prop);
            }

            switch (optionAttr)
            {
                case HelpOptionAttribute _:
                    App.OptionHelp = option;
                    break;
                case VersionOptionAttribute v:
                    App.OptionVersion = option;
                    App.ShortVersionGetter = () => v.Version;
                    break;
                default:
                    App.Options.Add(option);
                    break;
            }

            var setter = GetSetter(prop);

            switch (option.OptionType)
            {
                case CommandOptionType.MultipleValue:
                    var collectionParser = CollectionParserProvider.Default.GetParser(prop.PropertyType);
                    if (collectionParser == null)
                    {
                        throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                    }
                    OnBind(o => { setter.Invoke(o, collectionParser.Parse(option.LongName, option.Values)); });
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

        private void AddArgument(PropertyInfo prop, ArgumentAttribute argumentAttr)
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

            if (_argPropOrder.TryGetValue(argumentAttr.Order, out var otherProp))
            {
                throw new InvalidOperationException(
                    Strings.DuplicateArgumentPosition(argumentAttr.Order, prop, otherProp));
            }

            _argPropOrder.Add(argumentAttr.Order, prop);
            _argOrder.Add(argumentAttr.Order, argument);

            var setter = GetSetter(prop);

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
        
        private void OnBind(Action<object> onBind)
        {
            _binders.Add(onBind);
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
    }
}