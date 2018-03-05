// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Adds a <see cref="CommandArgument"/> for each <see cref="ArgumentAttribute"/>
    /// on the model type for <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class ArgumentAttributeConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var props = ReflectionHelper.GetProperties(context.ModelType);
            if (props == null)
            {
                return;
            }

            var argOrder = new SortedList<int, CommandArgument>();
            var argPropOrder = new Dictionary<int, PropertyInfo>();

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

                AddArgument(prop, argumentAttr, context, argOrder, argPropOrder);
            }

            foreach (var arg in argOrder)
            {
                if (context.Application.Arguments.Count > 0)
                {
                    var lastArg = context.Application.Arguments[context.Application.Arguments.Count - 1];
                    if (lastArg.MultipleValues)
                    {
                        throw new InvalidOperationException(
                            Strings.OnlyLastArgumentCanAllowMultipleValues(lastArg.Name));
                    }
                }

                context.Application.Arguments.Add(arg.Value);
            }
        }

        private void AddArgument(PropertyInfo prop,
            ArgumentAttribute argumentAttr,
            ConventionContext convention,
            SortedList<int, CommandArgument> argOrder,
            Dictionary<int, PropertyInfo> argPropOrder)
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

            if (argPropOrder.TryGetValue(argumentAttr.Order, out var otherProp))
            {
                throw new InvalidOperationException(
                    Strings.DuplicateArgumentPosition(argumentAttr.Order, prop, otherProp));
            }

            argPropOrder.Add(argumentAttr.Order, prop);
            argOrder.Add(argumentAttr.Order, argument);

            var setter = ReflectionHelper.GetPropertySetter(prop);

            if (argument.MultipleValues)
            {
                var collectionParser = CollectionParserProvider.Default.GetParser(prop.PropertyType);
                if (collectionParser == null)
                {
                    throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                }

                convention.Application.OnParsed(r =>
                {
                    if (r.SelectedCommand is IModelAccessor cmd)
                    {
                        setter.Invoke(cmd.GetModel(), collectionParser.Parse(argument.Name, argument.Values));
                    }
                });
            }
            else
            {
                var parser = ValueParserProvider.Default.GetParser(prop.PropertyType);
                if (parser == null)
                {
                    throw new InvalidOperationException(Strings.CannotDetermineParserType(prop));
                }

                convention.Application.OnParsed(r =>
                {
                    if (r.SelectedCommand is IModelAccessor cmd)
                    {
                        setter.Invoke(cmd.GetModel(), parser.Parse(argument.Name, argument.Value));
                    }
                });
            }
        }
    }
}
