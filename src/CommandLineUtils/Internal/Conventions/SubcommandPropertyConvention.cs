using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class SubcommandPropertyConvention : IAppConvention
    {
        public void Apply(CommandLineApplication app, Type modelType)
        {
            if (!(app is IModelProvider provider))
            {
                return;
            }

            var subcommandProp = modelType.GetTypeInfo().GetProperty("Subcommand", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (subcommandProp != null)
            {
                var setter = ReflectionHelper.GetPropertySetter(subcommandProp);
                app.OnParsed(r =>
                {
                    if (r.SelectedCommand is IModelProvider subcommandProvider)
                    {
                        setter.Invoke(provider.Model, subcommandProvider.Model);
                    }
                });
            }
        }
    }
}
