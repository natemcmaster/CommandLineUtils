// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Conventions;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Creates a <see cref="CommandLineApplication"/> as defined by attributes.
    /// </summary>
    internal class ReflectionAppBuilder<TModel>
        where TModel : class, new()
    {
        private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        private volatile bool _initialized;

        private readonly List<Action<TModel>> _binders = new List<Action<TModel>>();
        private readonly SortedList<int, CommandArgument> _argOrder = new SortedList<int, CommandArgument>();
        private readonly Dictionary<int, PropertyInfo> _argPropOrder = new Dictionary<int, PropertyInfo>();
        private readonly Dictionary<string, PropertyInfo> _shortOptions = new Dictionary<string, PropertyInfo>();
        private readonly Dictionary<string, PropertyInfo> _longOptions = new Dictionary<string, PropertyInfo>();

        public ReflectionAppBuilder()
            : this(new CommandLineApplication<TModel>())
        { }

        private ReflectionAppBuilder(CommandLineApplication<TModel> app)
        {
            App = app;
            App.Conventions
                .AddConvention(new CommandAttributeConvention())
                .AddConvention(new AppNameFromEntryAssemblyConvention())
                .AddConvention(new RemainingArgsPropertyConvention())
                .AddConvention(new SubcommandPropertyConvention())
                .AddConvention(new ParentPropertyConvention())
                .AddConvention(new VersionOptionFromMemberAttributeConvention());
            App.Initialize();
        }

        internal CommandLineApplication<TModel> App { get; }

        public BindResult Bind(CommandLineContext context)
        {
            EnsureInitialized();
            App.SetContext(context);

            var parseResult = App.Parse(context.Arguments);
            var command = parseResult.SelectedCommand;
            var bindResult = new BindResult
            {
                Command = parseResult.SelectedCommand,
                ParentTarget = App.Model,
                ValidationResult = command.GetValidationResult(),
            };

            if (command is IModelAccessor accessor)
            {
                bindResult.Target = accessor.GetModel();
            }

            return bindResult;
        }

        public void Initialize() => EnsureInitialized();

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            var type = typeof(TModel);
            var typeInfo = type.GetTypeInfo();

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
                    AddSubcommand(sub);
                }
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

            if (!string.IsNullOrEmpty(option.ShortName))
            {
                if (_shortOptions.TryGetValue(option.ShortName, out var otherProp))
                {
                    throw new InvalidOperationException(
                        Strings.OptionNameIsAmbiguous(option.ShortName, prop, otherProp));
                }
                _shortOptions.Add(option.ShortName, prop);
            }

            if (!string.IsNullOrEmpty(option.LongName))
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
                    App.OnParsed(_ =>
                        setter.Invoke(App.Model, collectionParser.Parse(option.LongName, option.Values)));
                    break;
                case CommandOptionType.SingleOrNoValue:
                    var valueTupleParser = ValueTupleParserProvider.Default.GetParser(prop.PropertyType);
                    if (valueTupleParser == null)
                    {
                        throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                    }
                    App.OnParsed(_ =>
                        setter.Invoke(App.Model, valueTupleParser.Parse(option.HasValue(), option.LongName, option.Value())));
                    break;
                case CommandOptionType.SingleValue:
                    var parser = ValueParserProvider.Default.GetParser(prop.PropertyType);
                    if (parser == null)
                    {
                        throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                    }
                    App.OnParsed(_ =>
                    {
                        var value = option.Value();
                        if (value == null)
                        {
                            return;
                        }
                        setter.Invoke(App.Model, parser.Parse(option.LongName, value));
                    });
                    break;
                case CommandOptionType.NoValue:
                    App.OnParsed(_ => setter.Invoke(App.Model, option.HasValue()));
                    break;
                default:
                    throw new NotImplementedException();
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

                App.OnParsed(_ => setter.Invoke(App.Model, collectionParser.Parse(argument.Name, argument.Values)));
            }
            else
            {
                var parser = ValueParserProvider.Default.GetParser(prop.PropertyType);
                if (parser == null)
                {
                    throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                }

                App.OnParsed(_ => setter.Invoke(App.Model, parser.Parse(argument.Name, argument.Value)));
            }
        }

        private void AddSubcommand(SubcommandAttribute subcommand)
        {
            var impl = s_addSubcommandMethod.MakeGenericMethod(subcommand.CommandType);
            try
            {
                impl.Invoke(this, new object[] { subcommand });
            }
            catch (TargetInvocationException ex)
            {
                // unwrap
                throw ex.InnerException ?? ex;
            }
        }

        private static readonly MethodInfo s_addSubcommandMethod
            = typeof(ReflectionAppBuilder<TModel>).GetRuntimeMethods().Single(m => m.Name == nameof(AddSubcommandImpl));

        private void AddSubcommandImpl<TSubCommand>(SubcommandAttribute subcommand)
            where TSubCommand : class, new()
        {
            if (App.Commands.Any(c => c.Name.Equals(subcommand.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(Strings.DuplicateSubcommandName(subcommand.Name));
            }

            var childApp = App.Command<TSubCommand>(subcommand.Name, subcommand.Configure);
            var builder = new ReflectionAppBuilder<TSubCommand>(childApp);
            builder.Initialize();
        }
    }
}
