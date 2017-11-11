// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
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
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            var applicationBuilder = new ReflectionAppBuilder<TApp>();
            var bindResult = applicationBuilder.Bind(console, args).GetBottomContext();
            if (IsShowingInfo(bindResult))
            {
                return 0;
            }

            var method = ReflectionHelper.GetExecuteMethod(bindResult.Target.GetType(), async: false);
            var arguments = BindParameters(console, bindResult, method);

            var result = method.Invoke(bindResult.Target, arguments);
            if (method.ReturnType == typeof(int))
            {
                return (int)result;
            }

            return 0;
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
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            var applicationBuilder = new ReflectionAppBuilder<TApp>(console);
            var bindResult = applicationBuilder.Bind(console, args).GetBottomContext();
            if (IsShowingInfo(bindResult))
            {
                return 0;
            }

            var method = ReflectionHelper.GetExecuteMethod(bindResult.Target.GetType(), async: true);
            var arguments = BindParameters(console, bindResult, method);

            var result = (Task)method.Invoke(bindResult.Target, arguments);
            if (method.ReturnType.GetTypeInfo().IsGenericType)
            {
                var task = (Task<int>)result;
                return await task;
            }

            await result;
            return 0;
        }

        private static object[] BindParameters(IConsole console, BindContext bindResult, MethodInfo method)
        {
            var methodParams = method.GetParameters();
            var arguments = new object[methodParams.Length];

            for (var i = 0; i < methodParams.Length; i++)
            {
                var methodParam = methodParams[i];

                if (typeof(CommandLineApplication).GetTypeInfo().IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = bindResult.App;
                }
                else if (typeof(IConsole).GetTypeInfo().IsAssignableFrom(methodParam.ParameterType))
                {
                    arguments[i] = console;
                }
                else
                {
                    throw new InvalidOperationException(Strings.UnsupportedOnExecuteParameterType(methodParam));
                }
            }

            return arguments;
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
