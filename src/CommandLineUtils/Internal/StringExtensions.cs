// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    internal static class StringExtensions
    {
        public static string ToKebabCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var sb = new StringBuilder();
            var i = 0;
            var addDash = false;

            for (; i < str.Length; i++)
            {
                var ch = str[i];
                if (char.IsLetterOrDigit(ch))
                {
                    addDash = !char.IsUpper(ch);
                    sb.Append(char.ToLowerInvariant(ch));
                    i++;
                    break;
                }
            }

            for (; i < str.Length; i++)
            {
                var ch = str[i];
                if (char.IsUpper(ch))
                {
                    if (addDash)
                    {
                        addDash = false;
                        sb.Append('-');
                    }

                    sb.Append(char.ToLowerInvariant(ch));
                }
                else if (char.IsLetterOrDigit(ch))
                {
                    addDash = true;
                    sb.Append(ch);
                }
                else
                {
                    addDash = false;
                    sb.Append('-');
                }
            }
            return sb.ToString();
        }

        public static string ToConstantCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var sb = new StringBuilder();
            var i = 0;
            var addUnderscore = false;

            for (; i < str.Length; i++)
            {
                var ch = str[i];
                if (char.IsLetterOrDigit(ch))
                {
                    addUnderscore = !char.IsUpper(ch);
                    sb.Append(char.ToUpperInvariant(ch));
                    i++;
                    break;
                }
            }

            for (; i < str.Length; i++)
            {
                var ch = str[i];
                if (char.IsUpper(ch))
                {
                    if (addUnderscore)
                    {
                        addUnderscore = false;
                        sb.Append('_');
                    }

                    sb.Append(char.ToUpperInvariant(ch));
                }
                else if (char.IsLetterOrDigit(ch))
                {
                    addUnderscore = true;
                    sb.Append(char.ToUpperInvariant(ch));
                }
                else
                {
                    addUnderscore = false;
                    sb.Append('_');
                }
            }
            return sb.ToString();
        }
    }
}
