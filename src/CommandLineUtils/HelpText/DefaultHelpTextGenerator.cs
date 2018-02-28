// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils.HelpText
{
    /// <summary>
    /// A default implementation of help text generation.
    /// </summary>
    public class DefaultHelpTextGenerator : IHelpTextGenerator
    {
        /// <summary>
        /// A singleton instance of <see cref="DefaultHelpTextGenerator" />.
        /// </summary>
        public static DefaultHelpTextGenerator Singleton { get; } = new DefaultHelpTextGenerator();

        private DefaultHelpTextGenerator() { }

        /// <inheritdoc />
        public void Generate(CommandLineApplication application, TextWriter output)
        {
            var nameAndVersion = application.GetFullNameAndVersion();
            if (!string.IsNullOrEmpty(nameAndVersion))
            {
                output.WriteLine(nameAndVersion);
                output.WriteLine();
            }

            output.Write("Usage:");
            var stack = new Stack<string>();
            for (var cmd = application; cmd != null; cmd = cmd.Parent)
            {
                stack.Push(cmd.Name);
            }

            while (stack.Count > 0)
            {
                output.Write(' ');
                output.Write(stack.Pop());
            }

            var arguments = application.Arguments.Where(a => a.ShowInHelpText).ToList();
            var options = application.GetOptions().Where(o => o.ShowInHelpText).ToList();
            var commands = application.Commands.Where(c => c.ShowInHelpText).ToList();

            if (arguments.Any())
            {
                output.Write(" [arguments]");
            }

            if (options.Any())
            {
                output.Write(" [options]");
            }

            if (commands.Any())
            {
                output.Write(" [command]");
            }

            if (application.AllowArgumentSeparator)
            {
                output.Write(" [[--] <arg>...]");
            }

            output.WriteLine();

            if (arguments.Any())
            {
                output.WriteLine();
                output.WriteLine("Arguments:");
                var maxArgLen = arguments.Max(a => a.Name.Length);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxArgLen + 2);

                var newLineWithMessagePadding = Environment.NewLine + new string(' ', maxArgLen + 4);

                foreach (var arg in arguments)
                {
                    var message = string.Format(outputFormat, arg.Name, arg.Description);
                    message = message.Replace(Environment.NewLine, newLineWithMessagePadding);

                    output.Write(message);
                    output.WriteLine();
                }
            }

            if (options.Any())
            {
                output.WriteLine();
                output.WriteLine("Options:");
                var maxOptLen = options.Max(o => o.Template?.Length ?? 0);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxOptLen + 2);

                var newLineWithMessagePadding = Environment.NewLine + new string(' ', maxOptLen + 4);

                foreach (var opt in options)
                {
                    var message = string.Format(outputFormat, opt.Template, opt.Description);
                    message = message.Replace(Environment.NewLine, newLineWithMessagePadding);

                    output.Write(message);
                    output.WriteLine();
                }
            }

            if (commands.Any())
            {
                output.WriteLine();
                output.WriteLine("Commands:");
                var maxCmdLen = commands.Max(c => c.Name?.Length ?? 0);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxCmdLen + 2);

                var newLineWithMessagePadding = Environment.NewLine + new string(' ', maxCmdLen + 4);

                foreach (var cmd in commands.OrderBy(c => c.Name))
                {
                    var message = string.Format(outputFormat, cmd.Name, cmd.Description);
                    message = message.Replace(Environment.NewLine, newLineWithMessagePadding);

                    output.Write(message);
                    output.WriteLine();
                }

                if (application.OptionHelp != null)
                {
                    output.WriteLine();
                    output.WriteLine($"Run '{application.Name} [command] --{application.OptionHelp.LongName}' for more information about a command.");
                }
            }

            output.Write(application.ExtendedHelpText);
            output.WriteLine();
        }
    }
}
