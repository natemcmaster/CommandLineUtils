// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Sets <see cref="CommandLineApplication.OptionVersion"/> using settings from
    /// <see cref="VersionOptionAttribute"/> on the model type of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class VersionOptionAttributeConvention : OptionAttributeConventionBase<VersionOptionAttribute>, IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            var modelAccessor = context.ModelAccessor;
            if (context.ModelType == null || modelAccessor == null)
            {
                return;
            }

            var versionOptionAttrOnType = context.ModelType.GetCustomAttribute<VersionOptionAttribute>();
            versionOptionAttrOnType?.Configure(context.Application);

            var props = ReflectionHelper.GetProperties(context.ModelType);

            VersionOptionAttribute? versionOptionAttr = null;
            PropertyInfo? property = null;

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<VersionOptionAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (versionOptionAttr != null)
                {
                    throw new InvalidOperationException(Strings.MultipleVersionOptionPropertiesFound);
                }

                if (versionOptionAttrOnType != null)
                {
                    throw new InvalidOperationException(Strings.VersionOptionOnTypeAndProperty);
                }

                versionOptionAttr = attr;
                property = prop;

                EnsureDoesNotHaveOptionAttribute(prop);
                EnsureDoesNotHaveHelpOptionAttribute(prop);
                EnsureDoesNotHaveArgumentAttribute(prop);
            }

            if (versionOptionAttr == null || property == null)
            {
                return;
            }

            var option = versionOptionAttr.Configure(context.Application);
            AddOption(context, option, property);
        }

        private static void EnsureDoesNotHaveOptionAttribute(PropertyInfo prop)
        {
            var regularOptionAttr = prop.GetCustomAttribute<OptionAttribute>();
            if (regularOptionAttr != null)
            {
                throw new InvalidOperationException(
                    Strings.BothOptionAndVersionOptionAttributesCannotBeSpecified(prop));
            }
        }

        private static void EnsureDoesNotHaveHelpOptionAttribute(PropertyInfo prop)
        {
            var versionOptionAttr = prop.GetCustomAttribute<HelpOptionAttribute>();
            if (versionOptionAttr != null)
            {
                throw new InvalidOperationException(
                    Strings.BothHelpOptionAndVersionOptionAttributesCannotBeSpecified(prop));
            }
        }
    }
}
