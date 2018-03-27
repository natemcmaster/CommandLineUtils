using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    internal static class StringDistance
    {

        //This is a modified version of Matt Enrights implementation
        // https://gist.github.com/wickedshimmy/449595


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
            if (s.Length == 0)
            {
                return t.Length;
            }
            if (t.Length == 0)
            {
                return s.Length;
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
                    var changes = new int[]
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
        public static double NormalizeDistance(int distance, int length)
        {
            return 1.0d - ((double) distance) / length;
        }

        public static int GetBestMatch(Func<string, string, int> distanceMethod,string value, string[] values)
        {
            var bestMatchValue = -1d;
            var bestMatchIndex = -1;
            for (var i = 0; i < values.Length; i++)
            {
                var distance = distanceMethod(value, values[i]);
                var length = value.Length > values[i].Length ? value.Length : values[i].Length;
                var normalizedDistance = StringDistance.NormalizeDistance(distance, length);
                if (bestMatchValue < normalizedDistance)
                {
                    bestMatchValue = normalizedDistance;
                    bestMatchIndex = i;
                }
            }
            return bestMatchIndex;
        }
    }
}
