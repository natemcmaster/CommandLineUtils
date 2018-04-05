// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.


namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// The command interface.
    /// </summary>
    public interface IAppInitializer

    {
        /// <summary>
        /// Initialize the application. 
        /// </summary>
        /// <param name="app"></param>
        void InitializeApp(CommandLineApplication app);
    }
}
