// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.IO
{
    /// <summary>
    /// An abstract prompt.
    /// </summary>
    public interface IPrompt
    {
        /// <summary>
        /// <see cref="McMaster.Extensions.CommandLineUtils.Prompt.GetYesNo"/>.
        /// </summary>
        bool GetYesNo(string prompt, bool defaultAnswer, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null);

        /// <summary>
        /// <see cref="McMaster.Extensions.CommandLineUtils.Prompt.GetString"/>.
        /// </summary>
        string GetString(string prompt, string defaultValue = null, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null);

        /// <summary>
        /// <see cref="McMaster.Extensions.CommandLineUtils.Prompt.GetPassword"/>.
        /// </summary>
        string GetPassword(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null);

#if NET45 || NETSTANDARD2_0
        /// <summary>
        /// <see cref="McMaster.Extensions.CommandLineUtils.Prompt.GetPasswordAsSecureString"/>.
        /// </summary>
        System.Security.SecureString GetPasswordAsSecureString(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null);
#elif NETSTANDARD1_6
#else
#error Target frameworks should be updated
#endif

    }
}
