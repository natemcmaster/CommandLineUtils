// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Sets a property named <c>RemainingArguments</c> or <c>RemainingArgs</c>
    /// on the model type on <see cref="CommandLineApplication{TModel}"/>
    /// to the value of <see cref="CommandLineApplication.RemainingArguments"/>.
    /// </summary>
    public class RemainingArgsPropertyConvention : IConvention
    {
        private const BindingFlags PropertyBindingFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            var modelAccessor = context.ModelAccessor;
            if (context.ModelType == null || modelAccessor == null)
            {
                return;
            }

            // Try to get setter from generated metadata first (AOT-friendly)
            var specialProperties = context.MetadataProvider?.SpecialProperties;
            var setter = specialProperties?.RemainingArgumentsSetter;
            var propType = specialProperties?.RemainingArgumentsType;

            // Fall back to reflection if no generated metadata
            if (setter == null)
            {
                var prop = context.ModelType.GetProperty("RemainingArguments", PropertyBindingFlags);
                prop ??= context.ModelType.GetProperty("RemainingArgs", PropertyBindingFlags);
                if (prop == null)
                {
                    return;
                }

                var reflectionSetter = ReflectionHelper.GetPropertySetter(prop);
                setter = (obj, val) => reflectionSetter(obj, val);
                propType = prop.PropertyType;
            }

            if (propType == typeof(string[]))
            {
                context.Application.OnParsingComplete(r =>
                    setter(modelAccessor.GetModel(), r.SelectedCommand.RemainingArguments.ToArray()));
                return;
            }

            if (!typeof(IReadOnlyList<string>).IsAssignableFrom(propType))
            {
                throw new InvalidOperationException(Strings.RemainingArgsPropsIsUnassignable(context.ModelType));
            }

            context.Application.OnParsingComplete(r =>
                setter(modelAccessor.GetModel(), r.SelectedCommand.RemainingArguments));
        }
    }
}
