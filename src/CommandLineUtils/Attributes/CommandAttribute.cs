// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents a command line application using attributes to define options and arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandAttribute : Attribute
    {
        private string[] _names = Util.EmptyArray<string>();

        /// <summary>
        /// Initializes a new <see cref="CommandAttribute"/>.
        /// </summary>
        public CommandAttribute()
        { }

        /// <summary>
        /// Initializes a new <see cref="CommandAttribute"/>.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        public CommandAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new <see cref="CommandAttribute"/>.
        /// </summary>
        /// <param name="names">The names of the command. The first name given is the primary name</param>
        public CommandAttribute(params string[] names)
        {
            _names = names;
        }

        /// <summary>
        /// The name of the command line application. When this is a subcommand, it is the name of the word used to invoke the subcommand.
        /// </summary>
        /// <seealso cref="CommandLineApplication.Name" />
        public string? Name
        {
            get => _names.Length > 0 ? _names[0] : null;
            set
            {
                if (value != null)
                {
                    _names = new[] { value };
                }
                else
                {
                    _names = Util.EmptyArray<string>();
                }
            }
        }

        /// <summary>
        /// THe names of the command. The first is the primary name. All other names are aliases.
        /// </summary>
        public IEnumerable<string> Names => _names;

        /// <summary>
        /// The full name of the command line application to show in help text.
        /// </summary>
        /// <seealso cref="CommandLineApplication.FullName" />
        public string? FullName { get; set; }

        /// <summary>
        /// A description of the command.
        /// </summary>
        /// <seealso cref="CommandLineApplication.Description"/>
        public string? Description { get; set; }

        /// <summary>
        /// Determines if this command appears in generated help text.
        /// </summary>
        /// <seealso cref="CommandLineApplication.ShowInHelpText"/>
        public bool ShowInHelpText { get; set; } = true;

        /// <summary>
        /// Additional text that appears at the bottom of generated help text.
        /// </summary>
        /// <seealso cref="CommandLineApplication.ExtendedHelpText"/>
        public string? ExtendedHelpText { get; set; }

        /// <summary>
        /// Set the behavior for how to handle unrecognized arguments.
        /// </summary>
        public UnrecognizedArgumentHandling UnrecognizedArgumentHandling
        {
            get => _unrecognizedArgumentHandling ?? UnrecognizedArgumentHandling.Throw;
            set => _unrecognizedArgumentHandling = value;
        }

        private UnrecognizedArgumentHandling? _unrecognizedArgumentHandling;

        /// <summary>
        /// Allow '--' to be used to stop parsing arguments.
        /// </summary>
        /// <seealso cref="CommandLineApplication.AllowArgumentSeparator"/>
        public bool AllowArgumentSeparator { get; set; }

        /// <summary>
        /// Treat arguments beginning as '@' as a response file.
        /// </summary>
        /// <seealso cref="CommandLineApplication.ResponseFileHandling"/>
        public ResponseFileHandling ResponseFileHandling { get; set; } = ResponseFileHandling.Disabled;

        /// <summary>
        /// The way arguments and options are matched.
        /// </summary>
        public StringComparison OptionsComparison { get; set; } = StringComparison.Ordinal;

        /// <summary>
        /// Specifies the culture used to convert values into types.
        /// </summary>
        public CultureInfo ParseCulture { get; set; } = CultureInfo.CurrentCulture;

        /// <summary>
        /// Whether a Pager should be used to display help text.
        /// </summary>
        public bool UsePagerForHelpText { get; set; } = false;

        /// <summary>
        /// <para>
        /// One or more options of <see cref="CommandOptionType.NoValue"/>, followed by at most one option that takes values, should be accepted when grouped behind one '-' delimiter.
        /// </para>
        /// <para>
        /// When true, the following are equivalent.
        ///
        /// <code>
        /// -abcXyellow
        /// -abcX=yellow
        /// -abcX:yellow
        /// -abc -X=yellow
        /// -ab -cX=yellow
        /// -a -b -c -Xyellow
        /// -a -b -c -X yellow
        /// -a -b -c -X=yellow
        /// -a -b -c -X:yellow
        /// </code>
        /// </para>
        /// <para>
        /// This defaults to true unless an option with a short name of two or more characters is added.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <seealso href="https://www.gnu.org/software/libc/manual/html_node/Argument-Syntax.html"/>
        /// </remarks>
        public bool ClusterOptions
        {
            get => _clusterOptions ?? true;
            set => _clusterOptions = value;
        }

        private bool? _clusterOptions;

        internal void Configure(CommandLineApplication app)
        {
            // this might have been set from SubcommandAttribute
            app.Name = Name ?? app.Name;

            foreach (var name in Names.Skip(1))
            {
                app.AddName(name);
            }

            app.AllowArgumentSeparator = AllowArgumentSeparator;
            app.Description = Description;
            app.ExtendedHelpText = ExtendedHelpText;
            app.FullName = FullName;
            app.ResponseFileHandling = ResponseFileHandling;
            app.ShowInHelpText = ShowInHelpText;
            app.OptionsComparison = OptionsComparison;
            app.ValueParsers.ParseCulture = ParseCulture;
            app.UsePagerForHelpText = UsePagerForHelpText;

            if (_clusterOptions.HasValue)
            {
                app.ClusterOptions = _clusterOptions.Value;
            }

            if (_unrecognizedArgumentHandling.HasValue)
            {
                app.UnrecognizedArgumentHandling = _unrecognizedArgumentHandling.Value;
            }
        }
    }
}
