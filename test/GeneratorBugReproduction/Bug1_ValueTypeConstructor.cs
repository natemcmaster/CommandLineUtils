// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;

namespace GeneratorBugReproduction
{
    /// <summary>
    /// BUG 1: Commands with value type constructor parameters fail to compile.
    ///
    /// The generator produces:
    ///   var p0_0 = _services.GetService(typeof(int)) as int;
    ///
    /// This causes compiler error CS0077:
    ///   "The 'as' operator must be used with a reference type or nullable value type"
    ///
    /// Expected: Should generate code that checks service != null, then casts.
    /// </summary>
    [Command(Name = "bug1", Description = "Command with value type constructor - should fail to compile")]
    public class Bug1_ValueTypeConstructor
    {
        public int Port { get; }
        public bool Verbose { get; }

        public Bug1_ValueTypeConstructor(int port, bool verbose)
        {
            Port = port;
            Verbose = verbose;
        }

        private int OnExecute()
        {
            System.Console.WriteLine($"Port: {Port}, Verbose: {Verbose}");
            return 0;
        }
    }
}
