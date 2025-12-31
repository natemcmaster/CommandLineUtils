// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace AotSample
{

    /// <summary>
    /// Console logger implementation.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public void Log(string message) => Console.WriteLine($"[LOG] {message}");
    }

}
