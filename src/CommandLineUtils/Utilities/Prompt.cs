// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Utilities for getting input from an interactive console.
    /// </summary>
    public static class Prompt
    {
        /// <summary>
        /// Gets a yes/no response from the console after displaying a <paramref name="prompt" />.
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
        /// Gets a console response from the console after displaying a <paramref name="prompt" />.
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
        /// Gets a response that contains a password. Input is masked with an asterisk.
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

        /// <summary>
        /// Gets an integer response from the console after displaying a <paramref name="prompt" />.
        /// </summary>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="defaultAnswer">If the user provides an empty response, which value should be returned</param>
        /// <param name="promptColor">The console color to display</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>The response as a number</returns>
        public static int GetInt(string prompt, int? defaultAnswer = null, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            do
            {
                Write(prompt, promptColor, promptBgColor);
                if (defaultAnswer.HasValue)
                {
                    Write($" [{defaultAnswer.Value}]", promptColor, promptBgColor);
                }
                Console.Write(' ');

                var resp = Console.ReadLine()?.ToLower()?.Trim();

                if (string.IsNullOrEmpty(resp))
                {
                    if (defaultAnswer.HasValue)
                    {
                        return defaultAnswer.Value;
                    }
                    else
                    {
                        Console.WriteLine("Please enter a valid number or press CTRL+C to exit.");
                        continue;
                    }
                }

                if (int.TryParse(resp, out var result))
                {
                    return result;
                }

                Console.WriteLine($"Invalid number '{resp}'. Please enter a valid number or press CTRL+C to exit.");
            }
            while (true);
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
