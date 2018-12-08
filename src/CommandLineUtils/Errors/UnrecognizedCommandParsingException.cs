// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        /// <param name="nearestMatches">The options or commands that </param>
        /// <param name="message"></param>
        public UnrecognizedCommandParsingException(CommandLineApplication command,
            IEnumerable<string> nearestMatches,
            string message)
            : base(command, message)
        {
            NearestMatches = nearestMatches ?? throw new ArgumentNullException(nameof(nearestMatches));
        }

        /// <summary>
        /// A collection of strings representing suggestions about similar and valid commands or options for the invalid
        /// argument that caused this <see cref="UnrecognizedCommandParsingException"/>.
        /// </summary>
        /// <remarks>
        /// This property always be empty <see cref="CommandLineApplication.MakeSuggestionsInErrorMessage"/> is false.
        /// </remarks>
        /// <value>This property get/set the suggestions for an invalid argument.</value>
        public IEnumerable<string> NearestMatches { get; }
    }
}
