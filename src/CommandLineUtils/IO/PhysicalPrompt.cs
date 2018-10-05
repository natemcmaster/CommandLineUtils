// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.IO
{
    /// <summary>
    /// An implementation of <see cref="IPrompt"/> that wraps <see cref="McMaster.Extensions.CommandLineUtils.Prompt"/>.
    /// </summary>
    public class PhysicalPrompt : IPrompt
    {
        /// <summary>
        /// A shared instance of <see cref="PhysicalPrompt"/>.
        /// </summary>
        public static IPrompt Singleton { get; } = new PhysicalPrompt();

        /// <summary>
        /// <see cref="McMaster.Extensions.CommandLineUtils.Prompt.GetYesNo"/>.
        /// </summary>
        public bool GetYesNo(string prompt, bool defaultAnswer, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
            => Prompt.GetYesNo(prompt, defaultAnswer, promptColor, promptBgColor);

        /// <summary>
        /// <see cref="McMaster.Extensions.CommandLineUtils.Prompt.GetString"/>.
        /// </summary>
        public string GetString(string prompt, string defaultValue = null, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
            => Prompt.GetString(prompt, defaultValue, promptColor, promptBgColor);

        /// <summary>
        /// <see cref="McMaster.Extensions.CommandLineUtils.Prompt.GetPassword"/>.
        /// </summary>
        public string GetPassword(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
            => Prompt.GetPassword(prompt, promptColor, promptBgColor);

#if NET45 || NETSTANDARD2_0
        /// <summary>
        /// <see cref="McMaster.Extensions.CommandLineUtils.Prompt.GetPasswordAsSecureString"/>.
        /// </summary>
        public System.Security.SecureString GetPasswordAsSecureString(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
            => Prompt.GetPasswordAsSecureString(prompt, promptColor, promptBgColor);
#elif NETSTANDARD1_6
#else
#error Target frameworks should be updated
#endif
    }
}
