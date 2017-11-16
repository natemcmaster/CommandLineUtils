// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Utilities for getting input from an interactive console
    /// </summary>
    public static class Prompt
    {
        /// <summary>
        /// Blocks on a yes/no response on the console from the user <paramref name="prompt" />.
        /// <para>
        /// The parsing is case insensitive. Valid responses include: yes, no, y, n.
        /// </para>
        /// </summary>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="defaultAnswer">If the user provides an empty response, which value should be returned</param>
        /// <param name="promptColor">The console color to display</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>True is 'yes'</returns>
        public static bool GetYesNo(string prompt, bool defaultAnswer, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            var answerHint = defaultAnswer ? "[Y/n]" : "[y/N]";
            do
            {
                Write($"{prompt} {answerHint}", promptColor, promptBgColor);
                Console.Write(' ');

                var resp = Console.ReadLine()?.ToLower()?.Trim();

                if (string.IsNullOrEmpty(resp))
                {
                    return defaultAnswer;
                }

                if (resp == "n" || resp == "no")
                {
                    return false;
                }

                if (resp == "y" || resp == "yes")
                {
                    return true;
                }

                Console.WriteLine($"Invalid response '{resp}'. Please answer 'y' or 'n' or CTRL+C to exit.");
            }
            while (true);
        }

        /// <summary>
        /// Blocks on a console response from the users after displaying <paramref name="prompt" />.
        /// </summary>
        /// <param name="prompt">The question to display on command line</param>
        /// <param name="defaultValue">If the user enters a blank response, return this value instead.</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>The response the user gave. Can be null or empty</returns>
        public static string GetString(string prompt, string defaultValue = null, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            if (defaultValue != null)
            {
                prompt = $"{prompt} [{defaultValue}]";
            }

            Write(prompt, promptColor, promptBgColor);
            Console.Write(' ');

            var resp = Console.ReadLine();

            if (!string.IsNullOrEmpty(resp))
            {
                return resp;
            }

            return defaultValue;
        }

        /// <summary>
        /// Blocks on a console response that contains a password. Input is masked with an asterisk.
        /// </summary>
        /// <param name="prompt">The question to display on command line</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>The password as plaintext. Can be null or empty.</returns>
        public static string GetPassword(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            Write(prompt, promptColor, promptBgColor);
            Console.Write(' ');

            var resp = new StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(intercept: true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        break;
                    case ConsoleKey.Backspace:
                        if (resp.Length > 0)
                        {
                            Console.Write("\b \b");
                            resp.Remove(resp.Length - 1, 1);
                        }
                        break;
                    default:
                        resp.Append(key.KeyChar);
                        Console.Write("*");
                        break;
                }
            }
            while (key.Key != ConsoleKey.Enter);

            return resp.ToString();
        }

        private static void Write(string value, ConsoleColor? foreground, ConsoleColor? background)
        {
            if (foreground.HasValue)
            {
                Console.ForegroundColor = foreground.Value;
            }

            if (background.HasValue)
            {
                Console.BackgroundColor = background.Value;
            }

            Console.Write(value);

            if (foreground.HasValue || background.HasValue)
            {
                Console.ResetColor();
            }
        }
    }
}
