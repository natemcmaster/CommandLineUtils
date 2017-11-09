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

        internal CommandLineApplication App { get; } = new CommandLineApplication();

        public T Execute<T>(string[] args)
            where T : class, new()
        {
            AddType<T>();
            
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

            var parsingOptionsAttr = typeInfo.GetCustomAttribute<CommandAttribute>();
            parsingOptionsAttr?.Configure(App);

            var helpOptionAttrOnType = typeInfo.GetCustomAttribute<HelpOptionAttribute>();
            helpOptionAttrOnType?.Configure(App);

            var versionOptionAttrOnType = typeInfo.GetCustomAttribute<VersionOptionAttribute>();
            versionOptionAttrOnType?.Configure(App);

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
            CommandOption option;
            switch (optionAttr)
            {
                case HelpOptionAttribute h:
                    option = h.Configure(App);
                    break;
                case OptionAttribute o:
                    option = o.Configure(App, prop);
                    break;
                case VersionOptionAttribute v:
                    option = v.Configure(App);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (option.OptionType == CommandOptionType.NoValue && prop.PropertyType != typeof(bool))
            {
                throw new InvalidOperationException(Strings.NoValueTypesMustBeBoolean);
            }

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

            var setter = ReflectionHelper.GetPropertySetter(prop);

            switch (option.OptionType)
            {
                case CommandOptionType.MultipleValue:
                    var collectionParser = CollectionParserProvider.Default.GetParser(prop.PropertyType);
                    if (collectionParser == null)
                    {
                        throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                    }
                    OnBind(o => 
                        setter.Invoke(o, collectionParser.Parse(option.LongName, option.Values)));
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
                    OnBind(o => 
                        setter.Invoke(o, option.HasValue()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddArgument(PropertyInfo prop, ArgumentAttribute argumentAttr)
        {
            var argument = argumentAttr.Configure(prop);
            
            if ((prop.PropertyType.IsArray
                 || typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(prop.PropertyType))
                && prop.PropertyType != typeof(string)
                && !argument.MultipleValues)
            {
                throw new InvalidOperationException(Strings.MultipleValuesArgumentShouldBeCollection);
            }
            
            if (_argPropOrder.TryGetValue(argumentAttr.Order, out var otherProp))
            {
                throw new InvalidOperationException(
                    Strings.DuplicateArgumentPosition(argumentAttr.Order, prop, otherProp));
            }

            _argPropOrder.Add(argumentAttr.Order, prop);
            _argOrder.Add(argumentAttr.Order, argument);

            var setter = ReflectionHelper.GetPropertySetter(prop);

            if (argument.MultipleValues)
            {
                var collectionParser = CollectionParserProvider.Default.GetParser(prop.PropertyType);
                if (collectionParser == null)
                {
                    throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                }

                OnBind(o => 
                    setter.Invoke(o, collectionParser.Parse(argument.Name, argument.Values)));
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
    }
}