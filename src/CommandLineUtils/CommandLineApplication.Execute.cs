// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
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
        /// </summary>
        /// <seealso cref="OptionAttribute" />
        /// <seealso cref="ArgumentAttribute" />
        /// <seealso cref="HelpOptionAttribute"/>
        /// <seealso cref="VersionOptionAttribute"/>
        /// <param name="context">The execution context.</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int Execute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TApp>(CommandLineContext context)
            where TApp : class
            => ExecuteAsync<TApp>(context).GetAwaiter().GetResult();

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <see cref="CommandLineContext.Arguments"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// </summary>
        /// <seealso cref="OptionAttribute" />
        /// <seealso cref="ArgumentAttribute" />
        /// <seealso cref="HelpOptionAttribute"/>
        /// <seealso cref="VersionOptionAttribute"/>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static async Task<int> ExecuteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TApp>(CommandLineContext context, CancellationToken cancellationToken = default)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            where TApp : class
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

            try
            {
                using var app = new CommandLineApplication<TApp>();
                app.SetContext(context);
                app.Conventions.UseDefaultConventions();
                return await app.ExecuteAsync(context.Arguments, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return s_exitCodeOperationCanceled;
            }
            catch (CommandParsingException ex)
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

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// </summary>
        /// <seealso cref="OptionAttribute" />
        /// <seealso cref="ArgumentAttribute" />
        /// <seealso cref="HelpOptionAttribute"/>
        /// <seealso cref="VersionOptionAttribute"/>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int Execute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TApp>(params string[] args)
            where TApp : class
            => Execute<TApp>(PhysicalConsole.Singleton, args);

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// </summary>
        /// <seealso cref="OptionAttribute" />
        /// <seealso cref="ArgumentAttribute" />
        /// <seealso cref="HelpOptionAttribute"/>
        /// <seealso cref="VersionOptionAttribute"/>
        /// <param name="console">The console to use</param>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int Execute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TApp>(IConsole console, params string[] args)
            where TApp : class
        {
            args ??= Array.Empty<string>();
            var context = new DefaultCommandLineContext(console, Directory.GetCurrentDirectory(), args);
            return Execute<TApp>(context);
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// </summary>
        /// <seealso cref="OptionAttribute" />
        /// <seealso cref="ArgumentAttribute" />
        /// <seealso cref="HelpOptionAttribute"/>
        /// <seealso cref="VersionOptionAttribute"/>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static Task<int> ExecuteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TApp>(params string[] args)
        where TApp : class
            => ExecuteAsync<TApp>(PhysicalConsole.Singleton, args);

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// </summary>
        /// <seealso cref="OptionAttribute" />
        /// <seealso cref="ArgumentAttribute" />
        /// <seealso cref="HelpOptionAttribute"/>
        /// <seealso cref="VersionOptionAttribute"/>
        /// <param name="args">The arguments</param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static Task<int> ExecuteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TApp>(string[] args, CancellationToken cancellationToken = default)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        where TApp : class
        {
            args ??= Array.Empty<string>();
            var context = new DefaultCommandLineContext(PhysicalConsole.Singleton, Directory.GetCurrentDirectory(), args);
            return ExecuteAsync<TApp>(context, cancellationToken);
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// </summary>
        /// <seealso cref="OptionAttribute" />
        /// <seealso cref="ArgumentAttribute" />
        /// <seealso cref="HelpOptionAttribute"/>
        /// <seealso cref="VersionOptionAttribute"/>
        /// <param name="console">The console to use</param>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static Task<int> ExecuteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TApp>(IConsole console, params string[] args)
            where TApp : class
        {
            args ??= Array.Empty<string>();
            var context = new DefaultCommandLineContext(console, Directory.GetCurrentDirectory(), args);
            return ExecuteAsync<TApp>(context);
        }
    }
}
