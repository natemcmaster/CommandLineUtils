// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Defines the kinds of inputs <see cref="CommandOption"/> accepts.
    /// </summary>
    public enum CommandOptionType
    {
        /// <summary>
        /// The option can be specified multiple times.
        /// <para>
        /// Example input: <c>--letter A --letter B --letter C</c>
        /// </para>
        /// </summary>
        MultipleValue,

        /// <summary>
        /// The option can only be specified once.
        /// <para>
        /// Example input: <c>--letter A</c>
        /// </para>
        /// <para>
        /// Example input: <c>--letter=A</c>
        /// </para>
        /// <para>
        /// Example input: <c>--letter:A</c>
        /// </para>
        /// </summary>
        SingleValue,

        /// <summary>
        /// The option can only be specified once, and may or may not have a value.
        /// <para>
        /// To disambiguate this from <see cref="NoValue"/>, values provided cannot be space-separated from the option name,
        /// but must use '=' or ':'
        /// </para>
        /// <para>
        /// Example input: <c>--log</c>
        /// </para>
        /// <para>
        /// Example input: <c>--log:verbose</c>
        /// </para>
        /// </summary>
        SingleOrNoValue,

        /// <summary>
        /// The option can only be specified once, and does not have a value.
        /// <para>
        /// Example input: <c>--no-commit</c>
        /// </para>
        /// </summary>
        NoValue
    }
}
