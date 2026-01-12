// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;

namespace GeneratorBugReproduction
{
    /// <summary>
    /// BUG 2: Commands with nullable array RemainingArguments use wrong code path.
    ///
    /// The generator uses string comparison:
    ///   if (sp.RemainingArgumentsPropertyType == "string[]")
    ///
    /// For nullable arrays (string[]?), the type string is "string[]?" which doesn't match.
    /// This causes the generator to use direct cast instead of conversion:
    ///   RemainingArguments = (string[]?)val
    ///
    /// This will cause InvalidCastException at runtime when IReadOnlyList is passed.
    ///
    /// Expected: Should use type analysis (IArrayTypeSymbol) not string comparison.
    /// </summary>
    [Command(Name = "bug2", Description = "Command with nullable array RemainingArguments")]
    [Subcommand(typeof(Bug2SubCommand))]
    public class Bug2_NullableArrayRemainingArgs
    {
        public string[]? RemainingArguments { get; set; }

        private int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 0;
        }
    }

    [Command(Name = "sub", Description = "Subcommand that uses RemainingArguments")]
    public class Bug2SubCommand
    {
        public string[]? RemainingArguments { get; set; }

        private int OnExecute()
        {
            if (RemainingArguments != null)
            {
                System.Console.WriteLine($"Args: {string.Join(", ", RemainingArguments)}");
            }
            return 0;
        }
    }
}
