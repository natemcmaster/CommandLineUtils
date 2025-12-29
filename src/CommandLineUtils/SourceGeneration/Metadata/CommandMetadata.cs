// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Metadata about a command from the [Command] attribute.
    /// </summary>
    public sealed class CommandMetadata
    {
        /// <summary>
        /// The primary name of the command.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Additional names/aliases for the command.
        /// </summary>
        public string[]? AdditionalNames { get; init; }

        /// <summary>
        /// The full name of the command to show in help text.
        /// </summary>
        public string? FullName { get; init; }

        /// <summary>
        /// A description of the command.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Whether this command appears in generated help text.
        /// </summary>
        public bool ShowInHelpText { get; init; } = true;

        /// <summary>
        /// Additional text that appears at the bottom of generated help text.
        /// </summary>
        public string? ExtendedHelpText { get; init; }

        /// <summary>
        /// How to handle unrecognized arguments.
        /// </summary>
        public UnrecognizedArgumentHandling? UnrecognizedArgumentHandling { get; init; }

        /// <summary>
        /// Whether to allow '--' to stop parsing arguments.
        /// </summary>
        public bool? AllowArgumentSeparator { get; init; }

        /// <summary>
        /// How to handle response files.
        /// </summary>
        public ResponseFileHandling? ResponseFileHandling { get; init; }

        /// <summary>
        /// The way arguments and options are matched.
        /// </summary>
        public StringComparison? OptionsComparison { get; init; }

        /// <summary>
        /// The culture used to convert values into types.
        /// </summary>
        public CultureInfo? ParseCulture { get; init; }

        /// <summary>
        /// Whether a Pager should be used to display help text.
        /// </summary>
        public bool? UsePagerForHelpText { get; init; }

        /// <summary>
        /// Whether options can be clustered behind one '-' delimiter.
        /// </summary>
        public bool? ClusterOptions { get; init; }

        /// <summary>
        /// Applies this metadata to a command line application.
        /// </summary>
        /// <param name="app">The application to configure.</param>
        public void ApplyTo(CommandLineApplication app)
        {
            if (Name != null)
            {
                app.Name = Name;
            }

            if (AdditionalNames != null)
            {
                foreach (var name in AdditionalNames)
                {
                    app.AddName(name);
                }
            }

            if (FullName != null)
            {
                app.FullName = FullName;
            }

            if (Description != null)
            {
                app.Description = Description;
            }

            app.ShowInHelpText = ShowInHelpText;

            if (ExtendedHelpText != null)
            {
                app.ExtendedHelpText = ExtendedHelpText;
            }

            if (UnrecognizedArgumentHandling.HasValue)
            {
                app.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.Value;
            }

            if (AllowArgumentSeparator.HasValue)
            {
                app.AllowArgumentSeparator = AllowArgumentSeparator.Value;
            }

            if (ResponseFileHandling.HasValue)
            {
                app.ResponseFileHandling = ResponseFileHandling.Value;
            }

            if (OptionsComparison.HasValue)
            {
                app.OptionsComparison = OptionsComparison.Value;
            }

            if (ParseCulture != null)
            {
                app.ValueParsers.ParseCulture = ParseCulture;
            }

            if (UsePagerForHelpText.HasValue)
            {
                app.UsePagerForHelpText = UsePagerForHelpText.Value;
            }

            if (ClusterOptions.HasValue)
            {
                app.ClusterOptions = ClusterOptions.Value;
            }
        }
    }
}
