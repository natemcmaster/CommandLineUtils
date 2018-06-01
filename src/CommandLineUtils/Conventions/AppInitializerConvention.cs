// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Calls a method with Application parameter when model is created.
    /// It can be handy for the attributed approach of creation of models, where the application parameters can be set only from constants.
    /// <see cref="IAppInitializer.InitializeApp" />
    /// </summary>
    public class AppInitializerConvention : IConvention
    {
        /// <summary>
        /// Initializes instance of <see cref="AppInitializerConvention" /> class. 
        /// </summary>
        public AppInitializerConvention()
        {                
        }

        /// <inheritdoc />
        public void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var model = context.ModelAccessor.GetModel();
            if (model is IAppInitializer appInitializerModel)
            {
                appInitializerModel.InitializeApp(context.Application);
            }
        }
    }
}
