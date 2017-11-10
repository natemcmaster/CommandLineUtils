// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

[Command("HelloWorldSample", ExtendedHelpText = CommandLineOptions.HelpText)]
class CommandLineOptions
{
    [Argument(0)]
    public string Verb { get; set; } = "Hello";

    [Option(Description = "A list of subjects. Multiple values allowed.")]
    public IReadOnlyList<string> Subjects { get; set; }

    [HelpOption]
    public bool IsHelp { get; set; }

    [VersionOption("99.99.99")]
    public bool IsVersion { get; set; }

    private const string HelpText
    = @"
Additional Info:
   This text is printed to the console when someone runs this app with `--help`.
";
}
