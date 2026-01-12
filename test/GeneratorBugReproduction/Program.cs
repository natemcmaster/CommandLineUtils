// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace GeneratorBugReproduction
{
    /// <summary>
    /// This program demonstrates bugs in the source generator.
    /// It is expected to FAIL TO COMPILE until the bugs are fixed.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This program should not compile with the current generator bugs.");
            Console.WriteLine("Bug 1: Value type constructor parameters cause CS0077");
            Console.WriteLine("Bug 2: Nullable array RemainingArguments use wrong conversion");
        }
    }
}
