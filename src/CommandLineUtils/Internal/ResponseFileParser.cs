// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class ResponseFileParser
    {
        public static IEnumerable<string> Parse(string filePath)
        {
            var rspLines = File.ReadAllLines(filePath);
            var args = new List<string>(capacity: rspLines.Length);
            var sb = new StringBuilder();
            foreach (var line in rspLines)
            {
                if (line.Length == 0) continue;
                if (line[0] == '#') continue;

                var breakOn = default(char?);

                var shouldCreateNewArg = false;

                for (var j = 0; j < line.Length; j++)
                {
                    var ch = line[j];
                    if (ch == '\\')
                    {
                        j++;
                        if (j >= line.Length)
                        {
                            // the backslash ended the document
                            sb.Append('\\');
                            break;
                        }

                        ch = line[j];

                        if (ch != '"' && ch != '\'')
                        {
                            // not a recognized special character, so add the backlash
                            sb.Append('\\');
                        }

                        sb.Append(ch);
                        continue;
                    }

                    if (breakOn == ch)
                    {
                        shouldCreateNewArg = true;
                        breakOn = null;
                        continue;
                    }

                    if (breakOn.HasValue)
                    {
                        sb.Append(ch);
                        continue;
                    }

                    if (char.IsWhiteSpace(ch))
                    {
                        if (sb.Length > 0 || shouldCreateNewArg)
                        {
                            shouldCreateNewArg = false;
                            args.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                    else if (ch == '"')
                    {
                        // the loop will search for the next unescaped "
                        breakOn = '"';
                    }
                    else if (ch == '\'')
                    {
                        // the loop will search for the next unescaped '
                        breakOn = '\'';
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                if (sb.Length > 0 || breakOn.HasValue || shouldCreateNewArg)
                {
                    // if we hit the end of the line, regardless of quoting, append everything as an arg
                    args.Add(sb.ToString());
                    sb.Clear();
                }
            }
            return args;
        }
    }
}
