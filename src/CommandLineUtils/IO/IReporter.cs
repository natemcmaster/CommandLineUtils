// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils
{
    public interface IReporter
    {
        void Verbose(string message);
        void Output(string message);
        void Warn(string message);
        void Error(string message);
    }
}