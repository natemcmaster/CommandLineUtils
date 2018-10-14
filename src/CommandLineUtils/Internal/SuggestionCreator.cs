// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace McMaster.Extensions.CommandLineUtils.Internal
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
        public static List<string> GetTopSuggestions(CommandLineApplication command, string input)
        {
            var candidates = GetCandidates(command).ToList();

            if (candidates.Count == 0)
            {
                return null;
            }

            return StringDistance.GetBestMatchesSorted(StringDistance.DamareuLevenshteinDistance,
                input,
                candidates.Select(c => c.CompareValue),
                0.33d)
                .ToList();
        }

        private static IEnumerable<Candidate> GetCandidates(CommandLineApplication command)
        {
            foreach (var cmd in command.Commands)
            {
                yield return new Candidate(cmd.Name, cmd.Name);
            }

            foreach (var option in command.GetOptions().Where(o => o.ShowInHelpText))
            {
                if (!string.IsNullOrEmpty(option.LongName))
                {
                    yield return new Candidate("--" + option.LongName, option.LongName);
                }

                if (!string.IsNullOrEmpty(option.ShortName))
                {
                    yield return new Candidate("-" + option.ShortName, option.ShortName);
                }

                if (!string.IsNullOrEmpty(option.SymbolName))
                {
                    yield return new Candidate("-" + option.SymbolName, option.SymbolName);
                }
            }
        }

        private struct Candidate
        {
            public Candidate(string displayValue, string compareValue)
            {
                DisplayValue = displayValue;
                CompareValue = compareValue;
            }

            public string DisplayValue { get; }
            public string CompareValue { get; }
        }
    }
}
