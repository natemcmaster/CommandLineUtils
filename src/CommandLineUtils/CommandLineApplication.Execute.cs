// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
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
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="CommandParsingException">Thrown when arguments cannot be parsed correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int Execute<TApp>(params string[] args)
            where TApp : class, new()
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
        /// <exception cref="CommandParsingException">Thrown when arguments cannot be parsed correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int Execute<TApp>(IConsole console, params string[] args)
            where TApp : class, new()
        {
            args = args ?? new string[0];
            var context = new DefaultCommandLineContext(console, Directory.GetCurrentDirectory(), args);
            return Execute<TApp>(context);
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <see cref="CommandLineContext.Arguments"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="CommandParsingException">Thrown when arguments cannot be parsed correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static int Execute<TApp>(CommandLineContext context)
            where TApp : class, new()
        {
            ValidateContextIsNotNull(context);

            var bindResult = Bind<TApp>(context);
            if (bindResult.Command.IsShowingInformation)
            {
                return HelpExitCode;
            }

            if (bindResult.ValidationResult != ValidationResult.Success)
            {
                return HandleValidationError(context, bindResult);
            }

            var invoker = ExecuteMethodInvoker.Create(bindResult.Target.GetType());
            switch (invoker)
            {
                case AsyncMethodInvoker asyncInvoker:
                    return asyncInvoker.ExecuteAsync(context, bindResult).GetAwaiter().GetResult();
                case SynchronousMethodInvoker syncInvoker:
                    return syncInvoker.Execute(context, bindResult);
                default:
                    throw new NotImplementedException();
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
        /// <exception cref="CommandParsingException">Thrown when arguments cannot be parsed correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static Task<int> ExecuteAsync<TApp>(params string[] args)
            where TApp : class, new()
            => ExecuteAsync<TApp>(PhysicalConsole.Singleton, args);

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="console">The console to use</param>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="CommandParsingException">Thrown when arguments cannot be parsed correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static Task<int> ExecuteAsync<TApp>(IConsole console, params string[] args)
            where TApp : class, new()
        {
            args = args ?? new string[0];
            var context = new DefaultCommandLineContext(console, Directory.GetCurrentDirectory(), args);
            return ExecuteAsync<TApp>(context);
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <see cref="CommandLineContext.Arguments"/>
        /// to all attributes on the type, and then invoking a method named "OnExecute" or "OnExecuteAsync" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="CommandParsingException">Thrown when arguments cannot be parsed correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static async Task<int> ExecuteAsync<TApp>(CommandLineContext context)
            where TApp : class, new()
        {
            ValidateContextIsNotNull(context);

            var bindResult = Bind<TApp>(context);
            if (bindResult.Command.IsShowingInformation)
            {
                return HelpExitCode;
            }

            if (bindResult.ValidationResult != ValidationResult.Success)
            {
                return HandleValidationError(context, bindResult);
            }

            var invoker = ExecuteMethodInvoker.Create(bindResult.Target.GetType());
            switch (invoker)
            {
                case AsyncMethodInvoker asyncInvoker:
                    return await asyncInvoker.ExecuteAsync(context, bindResult);
                case SynchronousMethodInvoker syncInvoker:
                    return syncInvoker.Execute(context, bindResult);
                default:
                    throw new NotImplementedException();
            }
        }

        private static int HandleValidationError(CommandLineContext context, BindResult bindResult)
        {
            const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var method = bindResult.Target
                .GetType()
                .GetTypeInfo()
                .GetMethod("OnValidationError", MethodFlags);

            if (method == null)
            {
                return bindResult.Command.DefaultValidationErrorHandler(bindResult.ValidationResult);
            }

            var arguments = ReflectionHelper.BindParameters(method, context, bindResult);
            var result = method.Invoke(bindResult.Target, arguments);
            if (method.ReturnType == typeof(int))
            {
                return (int)result;
            }

            return ValidationErrorExitCode;
        }

        private static BindResult Bind<TApp>(CommandLineContext context) where TApp : class, new()
        {
            var applicationBuilder = new ReflectionAppBuilder<TApp>();
            return applicationBuilder.Bind(context);
        }


        private static void ValidateContextIsNotNull(CommandLineContext context)
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
    }
}
