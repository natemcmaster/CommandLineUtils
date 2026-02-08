// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Creates suggestions based on an input string and a command object.
    /// </summary>
    internal static class SuggestionCreator
    {
        /// <summary>
        /// Gets a list of suggestions from sub commands and options of <paramref name="command"/> that are likely to
        /// fix the invalid argument <paramref name="input"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="input"></param>
        /// <returns>A list of string with suggestions or null if no suggestions were found</returns>
        public static IEnumerable<string> GetTopSuggestions(CommandLineApplication command, string input)
        {
            var candidates = GetCandidates(command).ToList();

            if (candidates.Count == 0)
            {
                return Enumerable.Empty<string>();
            }

            return StringDistance.GetBestMatchesSorted(StringDistance.DamareuLevenshteinDistance,
                input,
                candidates,
                0.33d);
        }

        private static IEnumerable<string> GetCandidates(CommandLineApplication command)
        {
            foreach (var cmd in command.Commands)
            {
                if (cmd.Name != null)
                {
                    yield return cmd.Name;
                }
            }

            foreach (var option in command.GetOptions().Where(o => o.ShowInHelpText))
            {
                if (!string.IsNullOrEmpty(option.LongName))
                {
                    yield return option.LongName;
                }

                if (!string.IsNullOrEmpty(option.ShortName))
                {
                    yield return option.ShortName;
                }

                if (!string.IsNullOrEmpty(option.SymbolName))
                {
                    yield return option.SymbolName;
                }
            }
        }
    }
}
