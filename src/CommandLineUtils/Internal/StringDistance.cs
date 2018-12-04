// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace McMaster.Extensions.CommandLineUtils
{
    internal static class StringDistance
    {
        /// <summary>
        /// Calculates the unnormalized (Damerau-)Levenshtein Distance of two strings
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <param name="damareu">Determites if the Damerau deviant is used</param>
        /// <returns></returns>
        private static int LevenshteinDistance(string s, string t, bool damareu)
        {
            // Short circuit so we don't waste cpu time
            if (s == t)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(s))
            {
                return !string.IsNullOrEmpty(t)
                    ? t.Length
                    : 0;
            }

            if (string.IsNullOrEmpty(t))
            {
                return !string.IsNullOrEmpty(s)
                    ? s.Length
                    : 0;
            }

            // Create matrix
            var matrix = new int[s.Length + 1, t.Length + 1];
            // Init matrix
            for (var i = 0; i <= s.Length; i++)
            {
                matrix[i, 0] = i;
            }

            for (var j = 0; j <= t.Length; j++)
            {
                matrix[0, j] = j;
            }

            //Transform matrix
            for (var i = 1; i <= s.Length; i++)
            {
                for (var j = 1; j <= t.Length; j++)
                {
                    var cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    var changes = new[]
                    {
                        //Insertion
                        matrix[i, j - 1] + 1,
                        //Deletion
                        matrix[i - 1, j] + 1,
                        //Substitution
                        matrix[i - 1, j - 1] + cost
                    };
                    var distance = changes.Min();
                    if (damareu)
                    {
                        if (i > 1 && j > 1 && s[i - 1] == t[j - 2] && s[i - 2] == t[j - 1])
                        {
                            // Transposition of two successive symbols
                            distance = Math.Min(distance, matrix[i - 2, j - 2] + cost);
                        }
                    }

                    matrix[i, j] = distance;
                }
            }

            return matrix[s.Length, t.Length];
        }

        /// <summary>
        /// Calculates the unnormalized Levenshtein Distance of two strings
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static int LevenshteinDistance(string s, string t)
        {
            return LevenshteinDistance(s, t, false);
        }

        /// <summary>
        /// Calculates the unnormalized Damareu-Levenshtein Distance of two strings
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static int DamareuLevenshteinDistance(string s, string t)
        {
            return LevenshteinDistance(s, t, true);
        }

        /// <summary>
        /// Normalizes the distance
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="length"></param>
        /// <returns>A value from 0 to 1 with 1 being a perfect match and zero none at all</returns>
        internal static double NormalizeDistance(int distance, int length)
        {
            if (length == 0)
            {
                return 0;
            }

            if (distance == 0)
            {
                return 1;
            }

            return 1.0d - distance / (double) length;
        }

        /// <summary>
        /// Gets a list of appropriate matches for <paramref name="value"/> in the given <paramref name="values"/>.
        /// Matches are sorted in ascending order, from the most likely match to less likely match using the <paramref name="distanceMethod"/>.
        /// The <paramref name="threshold"/> defines the minimum threshold for the string distance, below which words are not kept in the output list.
        /// </summary>
        /// <param name="distanceMethod">A method that calculates a string distance.</param>
        /// <param name="value">The value to compute the distance from.</param>
        /// <param name="values">The values to search through.</param>
        /// <param name="threshold">A threshold for word possible match.</param>
        /// <returns>The index of the best match or -1 when none is found</returns>
        internal static IEnumerable<string> GetBestMatchesSorted(Func<string, string, int> distanceMethod,
            string value,
            IEnumerable<string> values,
            double threshold)
        {
            if (distanceMethod == null || value == null || values == null)
            {
                return null;
            }

            return values
                .Where(v => v != null)
                .Select(stringValue =>
                {
                    var distance = distanceMethod(value, stringValue);
                    var length = Math.Max(value.Length, stringValue.Length);
                    var normalizedDistance = NormalizeDistance(distance, length);
                    return (stringValue, normalizedDistance);
                })
                .Where(candidate => candidate.normalizedDistance >= threshold)
                .OrderByDescending(candidate => candidate.normalizedDistance)
                .Select(candidate => candidate.stringValue);
        }
    }
}
