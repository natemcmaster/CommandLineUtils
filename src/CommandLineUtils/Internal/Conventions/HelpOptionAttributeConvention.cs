// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class HelpOptionAttributeConvention : OptionAttributeConventionBase<HelpOptionAttribute>, IConvention
    {
        public void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var helpOptionAttrOnType = context.ModelType.GetTypeInfo().GetCustomAttribute<HelpOptionAttribute>();
            helpOptionAttrOnType?.Configure(context.Application);

            var props = ReflectionHelper.GetProperties(context.ModelType);

            HelpOptionAttribute helpOptionAttr = null;
            PropertyInfo property = null;

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<HelpOptionAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (helpOptionAttr != null)
                {
                    throw new InvalidOperationException(Strings.MultipleHelpOptionPropertiesFound);
                }

                if (helpOptionAttrOnType != null)
                {
                    throw new InvalidOperationException(Strings.HelpOptionOnTypeAndProperty);
                }

                helpOptionAttr = attr;
                property = prop;

                EnsureDoesNotHaveVersionOptionAttribute(prop);
                EnsureDoesNotHaveOptionAttribute(prop);
                EnsureDoesNotHaveArgumentAttribute(prop);
            }

            if (helpOptionAttr == null)
            {
                return;
            }

            var option = helpOptionAttr.Configure(context.Application);
            AddOption(context, option, property);
        }

        private static void EnsureDoesNotHaveOptionAttribute(PropertyInfo prop)
        {
            var regularOptionAttr = prop.GetCustomAttribute<OptionAttribute>();
            if (regularOptionAttr != null)
            {
                throw new InvalidOperationException(
                    Strings.BothOptionAndHelpOptionAttributesCannotBeSpecified(prop));
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
    }
}
