// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// <para>
    /// Specifies options for how to handle response files. The parser treats arguments beginning with '@' as a file path to a response file.
    /// </para>
    /// <para>
    /// A response file contains additional arguments that will be treated as if they were passed in on the command line.
    /// Response files can have comments that begin with the # symbol.
    /// You cannot use the backslash character (\) to concatenate lines.
    /// </para>
    /// </summary>
    public enum ResponseFileHandling
    {
        /// <summary>
        /// Do not parse response files or treat arguments with '@' as a response file
        /// </summary>
        Disabled,

        /// <summary>
        /// <para>
        /// Multiple arguments may appear on one line. Arguments are separate by spaces.
        /// </para>
        /// <para>
        /// Double and single quotes can be used to wrap arguments containing spaces.
        /// </para>
        /// </summary>
        ParseArgsAsSpaceSeparated,

        /// <summary>
        /// <para>
        /// Each line in the file is treated as an argument, regardless of whitespace on the line.
        /// </para>
        /// <para>
        /// Lines beginning with # are skipped.
        /// </para>
        /// </summary>
        ParseArgsAsLineSeparated,
    }
}
