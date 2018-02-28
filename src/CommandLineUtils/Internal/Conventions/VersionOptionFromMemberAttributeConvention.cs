using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class VersionOptionFromMemberAttributeConvention : IAppConvention
    {
        public void Apply(CommandLineApplication app, Type modelType)
        {
            if (!(app is IModelProvider provider))
            {
                return;
            }

            var versionOptionFromMember = modelType.GetTypeInfo().GetCustomAttribute<VersionOptionFromMemberAttribute>();
            versionOptionFromMember?.Configure(app, modelType, () => provider.Model);
        }
    }
}
