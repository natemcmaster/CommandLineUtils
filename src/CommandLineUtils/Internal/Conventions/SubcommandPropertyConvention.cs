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
            if (subcommandProp == null)
            {
                return;
            }

            var setter = ReflectionHelper.GetPropertySetter(subcommandProp);
            app.OnParsed(r =>
            {
                var subCommand = r.SelectedCommand;
                while (subCommand != null)
                {
                    if (ReferenceEquals(subCommand.Parent, app))
                    {
                        if (subCommand is IModelProvider subCommandModel)
                        {
                            setter(provider.Model, subCommandModel.Model);
                        }
                        return;
                    }
                    subCommand = subCommand.Parent;
                }
            });
        }
    }
}
