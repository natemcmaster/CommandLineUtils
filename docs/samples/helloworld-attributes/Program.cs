// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils;

public class Program
{
    public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    [Option(Description = "The subject")]
    public string Subject { get; } = "world";

    [Option(ShortName = "n")]
    public int Count { get; } = 1;

    private void OnExecute()
    {
        for (var i = 0; i < Count; i++)
        {
            Console.WriteLine($"Hello {Subject}!");
        }
    }
}
