// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// The exception that is thrown when an invalid argument is given and when we can make suggestions
    /// about similar, valid commands or options.
    /// </summary>
    public class UnrecognizedCommandParsingException : CommandParsingException
    {

        /// <summary>
        /// Initializes an instance of <see cref="UnrecognizedCommandParsingException"/>.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="message"></param>
        public UnrecognizedCommandParsingException(CommandLineApplication command, string message) : base(command, message)
        { }

        /// <summary>
        /// A list of strings representing suggestions about similar and valid commands or options for the invalid
        /// argument that caused this <see cref="UnrecognizedCommandParsingException"/>.
        /// </summary>
        /// <remarks>
        /// This property is set if <see cref="ParserSettings.MakeSuggestionsInErrorMessage"/> is true
        /// </remarks>
        /// <value>This property get/set the suggestions for an invalid argument.</value>
        public List<string> NearestMatches { get; set; }
    }
}
