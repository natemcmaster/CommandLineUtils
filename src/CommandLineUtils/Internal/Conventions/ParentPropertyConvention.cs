using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class ParentPropertyConvention : IAppConvention
    {
        public void Apply(CommandLineApplication app, Type modelType)
        {
            if (!(app is IModelProvider provider))
            {
                return;
            }

            var parentProp = modelType.GetTypeInfo().GetProperty("Parent", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (parentProp == null)
            {
                return;
            }

            var setter = ReflectionHelper.GetPropertySetter(parentProp);
            app.OnParsed(r =>
            {
                var subcommand = r.SelectedCommand;
                while (subcommand != null)
                {
                    if (ReferenceEquals(app, subcommand))
                    {
                        if (subcommand.Parent is IModelProvider parentModel)
                        {
                            setter(provider.Model, parentModel.Model);
                        }
                        return;
                    }
                    subcommand = subcommand.Parent;
                }
            });
        }
    }
}
