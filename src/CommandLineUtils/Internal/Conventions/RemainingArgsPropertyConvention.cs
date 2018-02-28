// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class RemainingArgsPropertyConvention : IConvention
    {
        private const BindingFlags PropertyBindingFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        public void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var typeInfo = context.ModelType.GetTypeInfo();
            var prop = typeInfo.GetProperty("RemainingArguments", PropertyBindingFlags);
            prop = prop ?? typeInfo.GetProperty("RemainingArgs", PropertyBindingFlags);
            if (prop == null)
            {
                return;
            }

            var setter = ReflectionHelper.GetPropertySetter(prop);

            if (prop.PropertyType == typeof(string[]))
            {
                context.Application.OnParsed(r =>
                    setter(context.ModelAccessor.GetModel(), r.SelectedCommand.RemainingArguments.ToArray()));
                return;
            }

            if (!typeof(IReadOnlyList<string>).GetTypeInfo().IsAssignableFrom(prop.PropertyType))
            {
                throw new InvalidOperationException(Strings.RemainingArgsPropsIsUnassignable(typeInfo));
            }

            context.Application.OnParsed(r =>
                setter(context.ModelAccessor.GetModel(), r.SelectedCommand.RemainingArguments));
        }
    }
}
