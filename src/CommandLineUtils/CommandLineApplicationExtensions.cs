// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Helper methods for <see cref="CommandLineApplication"/>.
    /// </summary>
    public static class CommandLineApplicationExtensions
    {
        /// <summary>
        /// Adds a command line argument with values that should be parsable into <typeparamref name="T" />.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="multipleValues"></param>
        /// <returns></returns>
        public static CommandArgument<T> Argument<T>(this CommandLineApplication app, string name, string description, bool multipleValues = false)
            => app.Argument<T>(name, description, _ => { }, multipleValues);

        /// <summary>
        /// Adds a command-line option with values that should be parsable into <typeparamref name="T" />.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="template"></param>
        /// <param name="description"></param>
        /// <param name="optionType"></param>
        /// <returns></returns>
        public static CommandOption<T> Option<T>(this CommandLineApplication app, string template, string description, CommandOptionType optionType)
            => app.Option<T>(template, description, optionType, _ => { }, inherited: false);

        /// <summary>
        /// Adds a command-line option with values that should be parsable into <typeparamref name="T" />.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="template"></param>
        /// <param name="description"></param>
        /// <param name="optionType"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        public static CommandOption<T> Option<T>(this CommandLineApplication app, string template, string description, CommandOptionType optionType, bool inherited)
            => app.Option<T>(template, description, optionType, _ => { }, inherited);

        /// <summary>
        /// Adds a command-line option with values that should be parsable into <typeparamref name="T" />.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="template"></param>
        /// <param name="description"></param>
        /// <param name="optionType"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static CommandOption<T> Option<T>(this CommandLineApplication app, string template, string description, CommandOptionType optionType, Action<CommandOption> configuration)
            => app.Option<T>(template, description, optionType, configuration, inherited: false);

        /// <summary>
        /// Adds the help option with the template <c>-?|-h|--help</c>.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static CommandOption HelpOption(this CommandLineApplication app)
            => app.HelpOption(Strings.DefaultHelpTemplate);

        /// <summary>
        /// Adds the help option with the template <c>-?|-h|--help</c>.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        public static CommandOption HelpOption(this CommandLineApplication app, bool inherited)
            => app.HelpOption(Strings.DefaultHelpTemplate, inherited);

        /// <summary>
        /// Adds the verbose option with the template <c>-v|--verbose</c>.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static CommandOption VerboseOption(this CommandLineApplication app)
            => VerboseOption(app, "-v|--verbose");

        /// <summary>
        /// Adds the verbose option with the template <c>-v|--verbose</c>.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="template" />
        /// <returns></returns>
        public static CommandOption VerboseOption(this CommandLineApplication app, string template)
            => app.Option(template, "Show verbose output", CommandOptionType.NoValue, inherited: true);

        /// <summary>
        /// Sets an async command execution handler with a return code of <c>0</c>.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="action">An asynchronous action to invoke when the command is selected..</param>
        public static void OnExecuteAsync(this CommandLineApplication app, Func<CancellationToken, Task> action)
            => app.OnExecuteAsync(async ct =>
            {
                await action(ct);
                return 0;
            });

        /// <summary>
        /// Sets the command execution handler with a return code of <c>0</c>.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="action">An action to invoke when the command is selected.</param>
        public static void OnExecute(this CommandLineApplication app, Action action)
            => app.OnExecute(() =>
                {
                    action();
                    return 0;
                });

        /// <summary>
        /// Sets an action to invoke, but only when there is a validation error.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="action"></param>
        public static void OnValidationError(this CommandLineApplication app, Func<ValidationResult, int> action)
            => app.ValidationErrorHandler = action;

        /// <summary>
        /// Sets an action to invoke, but only when there is a validation error.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="action"></param>
        public static void OnValidationError(this CommandLineApplication app, Action<ValidationResult> action)
        {
            app.OnValidationError(r =>
            {
                action(r);
                return 1;
            });
        }

        /// <summary>
        /// Finds <see cref="AssemblyInformationalVersionAttribute"/> on <paramref name="assembly"/> and uses that
        /// to set <see cref="CommandLineApplication.OptionVersion"/>.
        /// <para>
        /// Uses the Version that is part of the <see cref="AssemblyName"/> of the specified assembly
        /// if no <see cref="AssemblyInformationalVersionAttribute"/> is applied.
        /// </para>
        /// </summary>
        /// <param name="app"></param>
        /// <param name="assembly"></param>
        /// <exception cref="ArgumentNullException">Either <paramref name="app"/> or <paramref name="assembly"/> is <c>null</c>.</exception>
        public static CommandOption VersionOptionFromAssemblyAttributes(this CommandLineApplication app, Assembly assembly)
            => VersionOptionFromAssemblyAttributes(app, Strings.DefaultVersionTemplate, assembly);

        /// <summary>
        /// Finds <see cref="AssemblyInformationalVersionAttribute"/> on <paramref name="assembly"/> and uses that
        /// to set <see cref="CommandLineApplication.OptionVersion"/>.
        /// <para>
        /// Uses the Version that is part of the <see cref="AssemblyName"/> of the specified assembly
        /// if no <see cref="AssemblyInformationalVersionAttribute"/> is applied.
        /// </para>
        /// </summary>
        /// <param name="app"></param>
        /// <param name="template"></param>
        /// <param name="assembly"></param>
        /// <exception cref="ArgumentNullException">Either <paramref name="app"/> or <paramref name="assembly"/> is <c>null</c>.</exception>
        public static CommandOption VersionOptionFromAssemblyAttributes(CommandLineApplication app, string template, Assembly assembly)
            => app.VersionOption(template, GetInformationalVersion(assembly));

        private static string? GetInformationalVersion(Assembly assembly)
        {
            var infoVersion = assembly
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            return string.IsNullOrWhiteSpace(infoVersion)
                ? assembly?.GetName().Version.ToString()
                : infoVersion;
        }
    }
}
