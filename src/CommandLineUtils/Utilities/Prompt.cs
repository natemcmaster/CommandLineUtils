// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Utilities for getting input from an interactive console.
    /// </summary>
    public static class Prompt
    {
        private const char Backspace = '\b';

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

                string? resp;
                using (ShowCursor())
                {
                    resp = Console.ReadLine()?.ToLower()?.Trim();
                }

                if (string.IsNullOrEmpty(resp))
                {
                    return defaultAnswer;
                }

                switch (resp)
                {
                    case "n":
                    case "no":
                        return false;
                    case "y":
                    case "yes":
                        return true;
                    default:
                        Console.WriteLine($"Invalid response '{resp}'. Please answer 'y' or 'n' or CTRL+C to exit.");
                        break;
                }
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
        public static string? GetString(string prompt, string? defaultValue = null, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            if (defaultValue != null)
            {
                prompt = $"{prompt} [{defaultValue}]";
            }

            Write(prompt, promptColor, promptBgColor);
            Console.Write(' ');

            string? resp;
            using (ShowCursor())
            {
                resp = Console.ReadLine();
            }

            return !string.IsNullOrEmpty(resp)
                ? resp
                : defaultValue;
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
            var resp = new StringBuilder();

            foreach (var key in ReadObfuscatedLine(prompt, promptColor, promptBgColor))
            {
                switch (key)
                {
                    case Backspace:
                        resp.Remove(resp.Length - 1, 1);
                        break;
                    default:
                        resp.Append(key);
                        break;
                }
            }

            return resp.ToString();
        }

        /// <summary>
        /// Gets a response as a SecureString object. Input is masked with an asterisk.
        /// </summary>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>A finalized SecureString object, may be empty.</returns>
        public static SecureString GetPasswordAsSecureString(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            var secureString = new SecureString();

            foreach (var key in ReadObfuscatedLine(prompt, promptColor, promptBgColor))
            {
                switch (key)
                {
                    case Backspace:
                        secureString.RemoveAt(secureString.Length - 1);
                        break;
                    default:
                        secureString.AppendChar(key);
                        break;
                }
            }

            secureString.MakeReadOnly();
            return secureString;
        }

        /// <summary>
        /// Base implementation of GetPassword and GetPasswordAsString. Prompts the user for
        /// a password and yields each key as the user inputs. Password is masked as input. Pressing Escape will reset the password
        /// by flushing the stream with backspace keys.
        /// </summary>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>A stream of characters as input by the user including Backspace for deletions.</returns>
        private static IEnumerable<char> ReadObfuscatedLine(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            const string WhiteOut = "\b \b";
            Write(prompt, promptColor, promptBgColor);
            Console.Write(' ');
            const ConsoleModifiers IgnoredModifiersMask = ConsoleModifiers.Alt | ConsoleModifiers.Control;
            var readChars = 0;
            ConsoleKeyInfo key;
            do
            {
                using (ShowCursor())
                {
                    key = Console.ReadKey(intercept: true);
                }

                if ((key.Modifiers & IgnoredModifiersMask) != 0)
                {
                    continue;
                }

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        break;
                    case ConsoleKey.Backspace:
                        if (readChars > 0)
                        {
                            Console.Write(WhiteOut);
                            --readChars;
                            yield return Backspace;
                        }
                        break;
                    case ConsoleKey.Escape:
                        // Reset the password
                        while (readChars > 0)
                        {
                            Console.Write(WhiteOut);
                            yield return Backspace;
                            --readChars;
                        }
                        break;
                    default:
                        readChars += 1;
                        Console.Write('*');
                        yield return key.KeyChar;
                        break;
                }
            }
            while (key.Key != ConsoleKey.Enter);
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

                string? resp;
                using (ShowCursor())
                {
                    resp = Console.ReadLine()?.ToLower()?.Trim();
                }

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

        private static IDisposable ShowCursor() => new CursorState();

        private class CursorState : IDisposable
        {
            private readonly bool _original;

            public CursorState()
            {
                try
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    _original = Console.CursorVisible;
#pragma warning restore CA1416 // Validate platform compatibility
                }
                catch
                {
                    // some platforms throw System.PlatformNotSupportedException
                    // Assume the cursor should be shown
                    _original = true;
                }

                TrySetVisible(true);
            }

            private void TrySetVisible(bool visible)
            {
                try
                {
                    Console.CursorVisible = visible;
                }
                catch
                {
                    // setting cursor may fail if output is piped or permission is denied.
                }
            }

            public void Dispose()
            {
                TrySetVisible(_original);
            }
        }
    }
}
