// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;

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
            // MetadataProvider is always available (generated or reflection-based via DefaultMetadataResolver)
            var provider = context.MetadataProvider;
            if (provider == null)
            {
                return;
            }

            ApplyFromMetadata(context, provider);
        }

        private void ApplyFromMetadata(ConventionContext context, ICommandMetadataProvider provider)
        {
            var argOrder = new SortedList<int, CommandArgument>();
            var argMetaByOrder = new Dictionary<int, ArgumentMetadata>();

            foreach (var argMeta in provider.Arguments)
            {
                // Check for duplicate argument positions
                if (argMetaByOrder.TryGetValue(argMeta.Order, out var existingMeta))
                {
                    // List the duplicate (current) property first, then the existing one
                    throw new InvalidOperationException(
                        Strings.DuplicateArgumentPosition(
                            argMeta.Order,
                            argMeta.PropertyName,
                            argMeta.DeclaringType,
                            existingMeta.PropertyName,
                            existingMeta.DeclaringType));
                }

                var argument = CreateArgumentFromMetadata(argMeta);
                argOrder.Add(argMeta.Order, argument);
                argMetaByOrder.Add(argMeta.Order, argMeta);
                AddArgumentFromMetadata(context, argument, argMeta);
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

                context.Application.AddArgument(arg.Value);
            }
        }

        private static CommandArgument CreateArgumentFromMetadata(ArgumentMetadata meta)
        {
            var argument = new CommandArgument
            {
                Name = meta.Name ?? meta.PropertyName,
                Description = meta.Description ?? string.Empty
            };

            argument.MultipleValues =
                meta.PropertyType.IsArray
                || (typeof(IEnumerable).IsAssignableFrom(meta.PropertyType)
                    && meta.PropertyType != typeof(string));

            argument.ShowInHelpText = meta.ShowInHelpText;

            // Set underlying type for help text generator (enum allowed values display)
            argument.UnderlyingType = meta.PropertyType;

            // Apply validation attributes from metadata
            foreach (var validator in meta.Validators)
            {
                argument.Validators.Add(new Validation.AttributeValidator(validator));
            }

            return argument;
        }

        private void AddArgumentFromMetadata(ConventionContext context, CommandArgument argument, ArgumentMetadata meta)
        {
            var modelAccessor = context.ModelAccessor;
            if (modelAccessor == null)
            {
                return;
            }

            var getter = meta.Getter;
            var setter = meta.Setter;

            if (argument.MultipleValues)
            {
                context.Application.OnParsingComplete(r =>
                {
                    var collectionParser = CollectionParserProvider.Default.GetParser(
                        meta.PropertyType,
                        context.Application.ValueParsers);
                    if (collectionParser == null)
                    {
                        throw new InvalidOperationException(
                            $"Cannot determine parser type for property '{meta.PropertyName}'");
                    }

                    if (argument.Values.Count == 0)
                    {
                        // Read the initial property value and use as default
                        if (!ReflectionHelper.IsSpecialValueTupleType(meta.PropertyType, out _))
                        {
                            if (getter(modelAccessor.GetModel()) is IEnumerable values
                                && meta.PropertyType != typeof(string))
                            {
                                var valueList = new System.Collections.Generic.List<string>();
                                foreach (var value in values)
                                {
                                    if (value != null)
                                    {
                                        valueList.Add(value.ToString() ?? string.Empty);
                                    }
                                }
                                if (valueList.Count > 0)
                                {
                                    argument.DefaultValue = string.Join(", ", valueList);
                                }
                            }
                        }
                        return;
                    }

                    if (r.SelectedCommand is IModelAccessor cmd)
                    {
                        setter(cmd.GetModel(), collectionParser.Parse(argument.Name, argument.Values));
                    }
                });
            }
            else
            {
                context.Application.OnParsingComplete(r =>
                {
                    var parser = context.Application.ValueParsers.GetParser(meta.PropertyType);
                    if (parser == null)
                    {
                        throw new InvalidOperationException(
                            $"Cannot determine parser type for property '{meta.PropertyName}'");
                    }

                    if (argument.Values.Count == 0)
                    {
                        // Read the initial property value and use as default
                        if (!ReflectionHelper.IsSpecialValueTupleType(meta.PropertyType, out _))
                        {
                            var value = getter(modelAccessor.GetModel());
                            if (value != null)
                            {
                                argument.DefaultValue = value.ToString();
                            }
                        }
                        return;
                    }

                    if (r.SelectedCommand is IModelAccessor cmd)
                    {
                        var model = cmd.GetModel();
                        setter(
                            model,
                            parser.Parse(
                                argument.Name,
                                argument.Value,
                                context.Application.ValueParsers.ParseCulture));
                    }
                });
            }
        }

    }
}
