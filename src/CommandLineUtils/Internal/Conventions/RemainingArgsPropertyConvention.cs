using System;
using System.Collections.Generic;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class RemainingArgsPropertyConvention : IAppConvention
    {
        private const BindingFlags PropertyBindingFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        public void Apply(CommandLineApplication app, Type modelType)
        {
            if (!(app is IModelProvider provider))
            {
                return;
            }

            var typeInfo = modelType.GetTypeInfo();
            var prop = typeInfo.GetProperty("RemainingArguments", PropertyBindingFlags);
            prop = prop ?? typeInfo.GetProperty("RemainingArgs", PropertyBindingFlags);
            if (prop == null)
            {
                return;
            }

            var setter = ReflectionHelper.GetPropertySetter(prop);

            if (prop.PropertyType == typeof(string[]))
            {
                app.OnParsed(_
                    => setter(provider.Model, app.RemainingArguments.ToArray()));
                return;
            }

            if (!typeof(IReadOnlyList<string>).GetTypeInfo().IsAssignableFrom(prop.PropertyType))
            {
                throw new InvalidOperationException(Strings.RemainingArgsPropsIsUnassignable(typeInfo));
            }

            app.OnParsed(_ =>
                setter(provider.Model, app.RemainingArguments));
        }
    }
}
