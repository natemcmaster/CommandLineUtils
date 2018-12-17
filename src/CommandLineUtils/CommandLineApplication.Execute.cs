// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Internal;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Describes a set of command line arguments, options, and execution behavior.
    /// <see cref="CommandLineApplication"/> can be nested to support subcommands.
    /// </summary>
    partial class CommandLineApplication
    {
        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <see cref="CommandLineContext.Arguments"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int Execute<TApp>(CommandLineContext context)
            where TApp : class
        {
            VerifyContext(context);

            try
            {
                using (var app = new CommandLineApplication<TApp>())
                {
                    app.SetContext(context);
                    app.Conventions.UseDefaultConventions();
                    return app.Execute(context.Arguments);
                }
            }
            catch (CommandParsingException ex)
            {
                return HandleCommandParsingException(context, ex);
            }
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int Execute<TApp>(params string[] args)
            where TApp : class
            => Execute<TApp>(PhysicalConsole.Singleton, args);

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="console">The console to use</param>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int Execute<TApp>(IConsole console, params string[] args)
            where TApp : class
        {
            args = args ?? new string[0];
            var context = new DefaultCommandLineContext(console, Directory.GetCurrentDirectory(), args);
            return Execute<TApp>(context);
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static Task<int> ExecuteAsync<TApp>(params string[] args)
            where TApp : class
            => ExecuteAsync<TApp>(PhysicalConsole.Singleton, default, args);

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation-token</param>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static Task<int> ExecuteAsync<TApp>(CancellationToken cancellationToken, params string[] args)
            where TApp : class
            => ExecuteAsync<TApp>(PhysicalConsole.Singleton, cancellationToken, args);

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="console">The console to use</param>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static Task<int> ExecuteAsync<TApp>(IConsole console, params string[] args)
            where TApp : class
            => ExecuteAsync<TApp>(console, default, args);

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="console">The console to use</param>
        /// <param name="cancellationToken">Optional cancellation-token</param>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static Task<int> ExecuteAsync<TApp>(IConsole console, CancellationToken cancellationToken,
            params string[] args)
            where TApp : class
        {
            args = args ?? new string[0];
            var context = new DefaultCommandLineContext(console, Directory.GetCurrentDirectory(), args);
            return ExecuteAsync<TApp>(context, cancellationToken);
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <see cref="CommandLineContext.Arguments"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">Optional cancellation-token</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static async Task<int> ExecuteAsync<TApp>(CommandLineContext context,
            CancellationToken cancellationToken = default)
            where TApp : class
        {
            VerifyContext(context);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var app = new CommandLineApplication<TApp>())
                {
                    app.SetContext(context);
                    app.Conventions.UseDefaultConventions();
                    cancellationToken.ThrowIfCancellationRequested();
                    return await app.ExecuteAsync(context.Arguments);
                }
            }
            catch (CommandParsingException ex)
            {
                return HandleCommandParsingException(context, ex);
            }
        }

        private static void VerifyContext(CommandLineContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Arguments == null)
            {
                throw new ArgumentNullException(nameof(context) + "." + nameof(context.Arguments));
            }

            if (context.WorkingDirectory == null)
            {
                throw new ArgumentNullException(nameof(context) + "." + nameof(context.WorkingDirectory));
            }

            if (context.Console == null)
            {
                throw new ArgumentNullException(nameof(context) + "." + nameof(context.Console));
            }
        }

        private static int HandleCommandParsingException(CommandLineContext context, CommandParsingException ex)
        {
            context.Console.Error.WriteLine(ex.Message);

            if (ex is UnrecognizedCommandParsingException uex && uex.NearestMatches.Any())
            {
                context.Console.Error.WriteLine();
                context.Console.Error.WriteLine("Did you mean this?");
                context.Console.Error.WriteLine("    " + uex.NearestMatches.First());
            }

            return ValidationErrorExitCode;
        }
    }
}
