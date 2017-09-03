// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Gathers messages with levels.
    /// </summary>
    public interface IReporter
    {
        /// <summary>
        /// Report a verbose message.
        /// </summary>
        /// <param name="message"></param>
        void Verbose(string message);

        /// <summary>
        /// Report console output.
        /// </summary>
        /// <param name="message"></param>
        void Output(string message);

        /// <summary>
        /// Report a warning.
        /// </summary>
        /// <param name="message"></param>
        void Warn(string message);

        /// <summary>
        /// Report an error.
        /// </summary>
        /// <param name="message"></param>
        void Error(string message);
    }
}
