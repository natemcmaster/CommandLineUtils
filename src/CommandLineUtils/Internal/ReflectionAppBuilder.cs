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

        public ReflectionAppBuilder()
            : this(new CommandLineApplication<TModel>())
        { }

        private ReflectionAppBuilder(CommandLineApplication<TModel> app)
        {
            App = app;
            App.Conventions.UseDefaultConventions();
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


            var props = ReflectionHelper.GetProperties(type);
            if (props != null)
            {
                AddProperties(props);
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

        private void AddProperties(IEnumerable<PropertyInfo> props)
        {
            foreach (var prop in props)
            {
                var argumentAttr = prop.GetCustomAttribute<ArgumentAttribute>();
                if (argumentAttr == null)
                {
                    continue;
                }

                if (prop.GetCustomAttributes().OfType<OptionAttributeBase>().Any())
                {
                    throw new InvalidOperationException(
                        Strings.BothOptionAndArgumentAttributesCannotBeSpecified(prop));
                }

                AddArgument(prop, argumentAttr);
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
