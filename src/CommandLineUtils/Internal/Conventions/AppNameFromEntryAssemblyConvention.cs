using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal class AppNameFromEntryAssemblyConvention : IAppConvention
    {
        public void Apply(CommandLineApplication app, Type modelType)
        {
            if (app.Name != null)
            {
                return;
            }

            var assembly = Assembly.GetEntryAssembly() == null
                ? modelType.GetTypeInfo().Assembly
                : Assembly.GetEntryAssembly();
            app.Name = assembly.GetName().Name;
        }
    }
}
