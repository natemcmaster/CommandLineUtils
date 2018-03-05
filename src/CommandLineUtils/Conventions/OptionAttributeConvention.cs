// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Adds an <see cref="CommandOption"/> to match each usage of <see cref="OptionAttribute"/>
    /// on the model type of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class OptionAttributeConvention : OptionAttributeConventionBase<OptionAttribute>, IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var props = ReflectionHelper.GetProperties(context.ModelType);
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<OptionAttribute>();
                if (attr == null)
                {
                    continue;
                }

                EnsureDoesNotHaveHelpOptionAttribute(prop);
                EnsureDoesNotHaveVersionOptionAttribute(prop);
                EnsureDoesNotHaveArgumentAttribute(prop);

                var option = attr.Configure(context.Application, prop);
                AddOption(context, option, prop);
            }
        }

        private static void EnsureDoesNotHaveVersionOptionAttribute(PropertyInfo prop)
        {
            var versionOptionAttr = prop.GetCustomAttribute<VersionOptionAttribute>();
            if (versionOptionAttr != null)
            {
                throw new InvalidOperationException(
                    Strings.BothHelpOptionAndVersionOptionAttributesCannotBeSpecified(prop));
            }
        }

        private static void EnsureDoesNotHaveHelpOptionAttribute(PropertyInfo prop)
        {
            var versionOptionAttr = prop.GetCustomAttribute<VersionOptionAttribute>();
            if (versionOptionAttr != null)
            {
                throw new InvalidOperationException(
                    Strings.BothHelpOptionAndVersionOptionAttributesCannotBeSpecified(prop));
            }
        }
    }
}
