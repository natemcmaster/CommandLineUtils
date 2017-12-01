// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Creates a <see cref="CommandLineApplication"/> as defined by attributes.
    /// </summary>
    internal class ReflectionAppBuilder<TTarget>
        where TTarget : class, new()
    {
        private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        private volatile bool _initialized;

        private readonly List<Action<TTarget>> _binders = new List<Action<TTarget>>();
        private readonly SortedList<int, CommandArgument> _argOrder = new SortedList<int, CommandArgument>();
        private readonly Dictionary<int, PropertyInfo> _argPropOrder = new Dictionary<int, PropertyInfo>();
        private readonly Dictionary<string, PropertyInfo> _shortOptions = new Dictionary<string, PropertyInfo>();
        private readonly Dictionary<string, PropertyInfo> _longOptions = new Dictionary<string, PropertyInfo>();

        public ReflectionAppBuilder()
            : this(PhysicalConsole.Singleton)
        { }

        public ReflectionAppBuilder(IConsole console)
            : this(new CommandLineApplication(console))
        { }

        private ReflectionAppBuilder(CommandLineApplication app)
        {
            App = app;
            App.OnExecute((Func<int>)OnExecute);
            App.OnValidationError(r =>
            {
                App.Invoke();
                var ctx = (BindContext)App.State;
                ctx.ValidationResult = r;
            });
        }

        internal CommandLineApplication App { get; }

        public BindContext Bind(IConsole console, string[] args)
        {
            EnsureInitialized();
            App.SetConsole(console);
            App.Execute(args);

            if (App.SelectedCommand != null)
            {
                // execution normally stops when help --help or --version is hit
                App.SelectedCommand.Invoke();
            }

            return (BindContext)App.State;
        }

        public void Initialize() => EnsureInitialized();

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            var type = typeof(TTarget);
            var typeInfo = type.GetTypeInfo();

            var parsingOptionsAttr = typeInfo.GetCustomAttribute<CommandAttribute>();
            parsingOptionsAttr?.Configure(App);

            if (parsingOptionsAttr?.ThrowOnUnexpectedArgument == false)
            {
                AddRemainingArgsProperty(typeInfo);
            }

            var helpOptionAttrOnType = typeInfo.GetCustomAttribute<HelpOptionAttribute>();
            helpOptionAttrOnType?.Configure(App);

            var versionOptionAttrOnType = typeInfo.GetCustomAttribute<VersionOptionAttribute>();
            versionOptionAttrOnType?.Configure(App);

            var props = typeInfo.GetProperties(PropertyBindingFlags);
            if (props != null)
            {
                AddProperties(props, helpOptionAttrOnType != null, versionOptionAttrOnType != null);
            }

            var subcommands = typeInfo.GetCustomAttributes<SubcommandAttribute>();
            if (subcommands != null)
            {
                foreach (var sub in subcommands)
                {
                    AddSubcommand(type, sub);
                }
            }
        }

        private void AddRemainingArgsProperty(TypeInfo typeInfo)
        {
            var prop = typeInfo.GetProperty("RemainingArguments", PropertyBindingFlags);
            if (prop == null)
            {
                return;
            }

            var setter = ReflectionHelper.GetPropertySetter(prop);

            if (prop.PropertyType == typeof(string[]))
            {
                OnBind(o
                    => setter(o, App.RemainingArguments.ToArray()));
                return;
            }

            if (!typeof(IReadOnlyList<string>).GetTypeInfo().IsAssignableFrom(prop.PropertyType))
            {
                throw new InvalidOperationException(Strings.RemainingArgsPropsIsUnassignable(typeInfo));
            }

            OnBind(o =>
                setter(o, App.RemainingArguments));
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

            foreach (var attr in prop.GetCustomAttributes().OfType<ValidationAttribute>())
            {
                option.Validators.Add(new AttributeValidator(attr));
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

            foreach (var attr in prop.GetCustomAttributes().OfType<ValidationAttribute>())
            {
                argument.Validators.Add(new AttributeValidator(attr));
            }

            argument.MultipleValues =
                prop.PropertyType.IsArray
                || (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(prop.PropertyType)
                    && prop.PropertyType != typeof(string));

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

        private void OnBind(Action<TTarget> onBind)
        {
            _binders.Add(onBind);
        }

        private int OnExecute()
        {
            App.Parent?.Invoke();

            var ctx = new BindContext
            {
                App = App,
                Target = new TTarget(),
            };

            if (App.Parent?.State is BindContext parentCtx)
            {
                parentCtx.Child = ctx;
            }

            App.State = ctx;

            foreach (var binder in _binders)
            {
                binder((TTarget)ctx.Target);
            }

            return 0;
        }

        private void AddSubcommand(Type parent, SubcommandAttribute subcommand)
        {
            var impl = AddSubcommandMethod.MakeGenericMethod(subcommand.CommandType);
            impl.Invoke(this, new object[] { parent, subcommand });
        }

        private static readonly MethodInfo AddSubcommandMethod
            = typeof(ReflectionAppBuilder<TTarget>).GetRuntimeMethods().Single(m => m.Name == nameof(AddSubcommandImpl));

        private void AddSubcommandImpl<TSubCommand>(Type parent, SubcommandAttribute subcommand)
            where TSubCommand : class, new()
        {
            var parentApp = App;
            var childApp = App.Command(subcommand.Name, subcommand.Configure);

            var builder = new ReflectionAppBuilder<TSubCommand>(childApp);
            builder.Initialize();

            var subcommandProp = parent.GetTypeInfo().GetProperty("Subcommand", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (subcommandProp != null)
            {
                var setter = ReflectionHelper.GetPropertySetter(subcommandProp);
                builder.OnBind(o =>
                {
                    if (parentApp.State is BindContext ctx)
                    {
                        setter.Invoke(ctx.Target, o);
                    }
                });
            }

            var parentProp = subcommand.CommandType.GetTypeInfo().GetProperty("Parent", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (parentProp != null)
            {
                var setter = ReflectionHelper.GetPropertySetter(parentProp);
                builder.OnBind(o =>
                {
                    if (parentApp.State is BindContext ctx)
                    {
                        setter.Invoke(o, ctx.Target);
                    }
                });
            }
        }
    }
}
