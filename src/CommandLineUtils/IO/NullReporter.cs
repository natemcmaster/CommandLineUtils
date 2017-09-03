// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// A reporter that does nothing.
    /// </summary>
    public class NullReporter : IReporter
    {
        private NullReporter()
        { }

        /// <summary>
        /// A shared instance of <see cref="NullReporter"/>.
        /// </summary>
        public static IReporter Singleton { get; } = new NullReporter();

        /// <inheritdoc />
        public void Verbose(string message)
        { }

        /// <inheritdoc />
        public void Output(string message)
        { }

        /// <inheritdoc />
        public void Warn(string message)
        { }

        /// <inheritdoc />
        public void Error(string message)
        { }
    }
}
