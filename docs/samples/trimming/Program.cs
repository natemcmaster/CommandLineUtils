// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils;

// Reproduces https://github.com/natemcmaster/CommandLineUtils/issues/539:
// a trimmed, self-contained, single-file publish using CommandLineApplication.Execute<TApp>
// with an attribute-based command model. Before the fix, a trimmed publish either warned
// during publish (when EnableTrimAnalyzer is set) or threw at run time with
// "No method named 'OnExecute' or 'OnExecuteAsync' could be found." /
// "Could not find any public constructors of type 'MyCommand'."
return CommandLineApplication.Execute<MyCommand>(args);

internal class MyCommand
{
    [Option(Description = "The subject to greet")]
    public string Subject { get; } = "world";

    private int OnExecute()
    {
        Console.WriteLine($"Hello, {Subject}!");
        return 0;
    }
}
