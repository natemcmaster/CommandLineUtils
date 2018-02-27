using System;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal interface IAppConvention
    {
        void Apply(CommandLineApplication app, Type modelType);
    }
}
