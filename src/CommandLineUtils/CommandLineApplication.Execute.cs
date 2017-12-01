// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;

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
        /// to all attributes on the type, and then invoking a method named "Execute" if it exists.
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
        /// to all attributes on the type, and then invoking a method named "Execute" if it exists.
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
            var bindResult = Bind<TApp>(console, args);
            if (IsShowingInfo(bindResult))
            {
                return 0;
            }

            if (bindResult.ValidationResult != ValidationResult.Success)
            {
                return HandleValidationError<TApp>(console, bindResult);
            }

            var invoker = ExecuteMethodInvoker.Create(bindResult.Target.GetType());
            switch (invoker)
            {
                case AsyncMethodInvoker asyncInvoker:
                    return asyncInvoker.ExecuteAsync(console, bindResult).GetAwaiter().GetResult();
                case SynchronousMethodInvoker syncInvoker:
                    return syncInvoker.Execute(console, bindResult);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TApp"/>, matching <paramref name="args"/>
        /// to all attributes on the type, and then invoking a method named "Execute" if it exists.
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
        /// to all attributes on the type, and then invoking a method named "Execute" if it exists.
        /// See <seealso cref="OptionAttribute" />, <seealso cref="ArgumentAttribute" />,
        /// <seealso cref="HelpOptionAttribute"/>, and <seealso cref="VersionOptionAttribute"/>.
        /// </summary>
        /// <param name="console">The console to use</param>
        /// <param name="args">The arguments</param>
        /// <typeparam name="TApp">A type that should be bound to the arguments.</typeparam>
        /// <exception cref="CommandParsingException">Thrown when arguments cannot be parsed correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when attributes are incorrectly configured.</exception>
        /// <returns>The process exit code</returns>
        public static async Task<int> ExecuteAsync<TApp>(IConsole console, params string[] args)
            where TApp : class, new()
        {
            var bindResult = Bind<TApp>(console, args);
            if (IsShowingInfo(bindResult))
            {
                return 0;
            }

            if (bindResult.ValidationResult != ValidationResult.Success)
            {
                return HandleValidationError<TApp>(console, bindResult);
            }

            var invoker = ExecuteMethodInvoker.Create(bindResult.Target.GetType());
            switch (invoker)
            {
                case AsyncMethodInvoker asyncInvoker:
                    return await asyncInvoker.ExecuteAsync(console, bindResult);
                case SynchronousMethodInvoker syncInvoker:
                    return syncInvoker.Execute(console, bindResult);
                default:
                    throw new NotImplementedException();
            }
        }

        private static int HandleValidationError<TApp>(IConsole console, BindContext bindResult)
        {
            var method = typeof(TApp).GetTypeInfo().GetMethod("OnValidationError", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                return bindResult.App.DefaultValidationErrorHandler(bindResult.ValidationResult);
            }

            var arguments = ReflectionHelper.BindParameters(method, console, bindResult);
            var result = method.Invoke(bindResult.Target, arguments);
            if (method.ReturnType == typeof(int))
            {
                return (int)result;
            }

            return 1;
        }

        private static BindContext Bind<TApp>(IConsole console, string[] args) where TApp : class, new()
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            var applicationBuilder = new ReflectionAppBuilder<TApp>();
            var bindResult = applicationBuilder.Bind(console, args).GetBottomContext();
            return bindResult;
        }

        private static bool IsShowingInfo(BindContext bindResult)
        {
            if (bindResult.App.IsShowingInformation)
            {
                if (bindResult.App.OptionHelp?.HasValue() == true && bindResult.App.StopParsingAfterHelpOption)
                {
                    return true;
                }

                if (bindResult.App.OptionVersion?.HasValue() == true && bindResult.App.StopParsingAfterVersionOption)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
