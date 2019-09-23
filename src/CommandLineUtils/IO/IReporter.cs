// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary> Gathers messages with levels. </summary>
    public interface IReporter
    {
        /// <summary> Report a verbose message. </summary>
        void Verbose(string message);

        void Output(string message);

        void Warn(string message);

        void Error(string message);
    }
}
