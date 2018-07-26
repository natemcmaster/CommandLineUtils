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
        public static string GetTopSuggestion(CommandLineApplication command, string input)
        {
            var candidates = GetCandidates(command).ToList();

            if (candidates.Count == 0)
            {
                return null;
            }

            var bestMatch = StringDistance.GetBestMatchIndex(StringDistance.DamareuLevenshteinDistance,
                input,
                candidates.Select(c => c.CompareValue).ToArray(),
                0.33d);

            return bestMatch > -1
                ? candidates[bestMatch].DisplayValue
                : null;
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
