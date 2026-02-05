// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace McMaster.Extensions.CommandLineUtils.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// A wrapper around <see cref="string.IsNullOrEmpty(string)"/> that allows proper nullability annotation.
        /// This is a workaround because .NET Framework assemblies are not nullability annotated.
        /// </summary>
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);
        /// <summary>
        /// A wrapper around <see cref="string.IsNullOrWhiteSpace(string)"/> that allows proper nullability annotation.
        /// This is a workaround because .NET Framework assemblies are not nullability annotated.
        /// </summary>
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);
    }
}
