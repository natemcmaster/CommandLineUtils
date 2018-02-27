using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class CommandAttributeConvention : IAppConvention
    {
        public void Apply(CommandLineApplication app, Type modelType)
        {
            var attribute = modelType.GetTypeInfo().GetCustomAttribute<CommandAttribute>();
            attribute?.Configure(app);
        }
    }
}
