// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace AotSample
{

    /// <summary>
    /// Simple logger service interface for testing DI.
    /// </summary>
    public interface ILogger
    {

        void Log(string message);

    }

}
