// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace McMaster.Extensions.CommandLineUtils.HelpText
{
    /// <summary>
    /// A default implementation of help text generation.
    /// </summary>
    public class DefaultHelpTextGenerator : IHelpTextGenerator
    {
        /// <summary>
        /// The number of spaces between columns.
        /// </summary>
        protected const int ColumnSeparatorLength = 2;
        private int? _maxLineLength = null;

        /// <summary>
        /// The hanging indent writer used for formatting indented and wrapped
        /// descriptions for options and arguments.
        /// </summary>
        protected HangingIndentWriter? IndentWriter { get; set; }

        /// <summary>
        /// A singleton instance of <see cref="DefaultHelpTextGenerator" />.
        /// </summary>
        public static DefaultHelpTextGenerator Singleton { get; } = new DefaultHelpTextGenerator();

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultHelpTextGenerator"/>.
        /// </summary>
        public DefaultHelpTextGenerator() { }

        /// <summary>
        /// Determines if commands are ordered by name in generated help text
        /// </summary>
        public bool SortCommandsByName { get; set; } = true;

        /// <summary>
        /// Override the console width disregarding any value from the executing environment.
        /// </summary>
        public int? MaxLineLength
        {
            get => _maxLineLength;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Value must be greater than zero", nameof(value));
                }

                _maxLineLength = value;
            }
        }

        /// <inheritdoc />
        public virtual void Generate(CommandLineApplication application, TextWriter output)
        {
            GenerateHeader(application, output);
            GenerateBody(application, output);
            GenerateFooter(application, output);
        }

        /// <summary>
        /// Generate the first few lines of help output text
        /// </summary>
        /// <param name="application">The app</param>
        /// <param name="output">Help text output</param>
        protected virtual void GenerateHeader(
            CommandLineApplication application,
            TextWriter output)
        {
            var nameAndVersion = application.GetFullNameAndVersion();
            if (!string.IsNullOrEmpty(nameAndVersion))
            {
                output.WriteLine(nameAndVersion);
                output.WriteLine();
            }

            if (!string.IsNullOrEmpty(application.Description))
            {
                output.WriteLine(application.Description);
                output.WriteLine();
            }
        }

        /// <summary>
        /// Generate detailed help information
        /// </summary>
        /// <param name="application">The application</param>
        /// <param name="output">Help text output</param>
        protected virtual void GenerateBody(
            CommandLineApplication application,
            TextWriter output)
        {
            var arguments = application.Arguments.Where(a => a.ShowInHelpText).ToList();
            var options = application.GetOptions().Where(o => o.ShowInHelpText).ToList();
            var commands = application.Commands.Where(c => c.ShowInHelpText).ToList();

            var firstColumnWidth = ColumnSeparatorLength + Math.Max(
                arguments.Count > 0 ? arguments.Max(a => a.Name?.Length ?? 0) : 0,
                Math.Max(
                    options.Count > 0 ? options.Max(o => Format(o).Length) : 0,
                    commands.Count > 0 ? commands.Max(c => c.Name?.Length ?? 0) : 0));

            IndentWriter = new HangingIndentWriter(firstColumnWidth + ColumnSeparatorLength, maxLineLength: TryGetConsoleWidth());

            GenerateUsage(application, output, arguments, options, commands);
            GenerateArguments(application, output, arguments, firstColumnWidth);
            GenerateOptions(application, output, options, firstColumnWidth);
            GenerateCommands(application, output, commands, firstColumnWidth);
        }

        /// <summary>
        /// Generate the line that shows usage
        /// </summary>
        /// <param name="application">The app</param>
        /// <param name="output">Help text output</param>
        /// <param name="visibleArguments">Arguments not hidden from help text</param>
        /// <param name="visibleOptions">Options not hidden from help text</param>
        /// <param name="visibleCommands">Commands not hidden from help text</param>
        protected virtual void GenerateUsage(
            CommandLineApplication application,
            TextWriter output,
            IReadOnlyList<CommandArgument> visibleArguments,
            IReadOnlyList<CommandOption> visibleOptions,
            IReadOnlyList<CommandLineApplication> visibleCommands)
        {
            output.Write("Usage:");
            var stack = new Stack<string?>();
            for (var cmd = application; cmd != null; cmd = cmd.Parent)
            {
                stack.Push(cmd.Name);
            }

            while (stack.Count > 0)
            {
                output.Write(' ');
                output.Write(stack.Pop());
            }

            if (visibleCommands.Any())
            {
                output.Write(" [command]");
            }

            if (visibleOptions.Any())
            {
                output.Write(" [options]");
            }

            foreach (var argument in visibleArguments)
            {
                output.Write(" <");
                output.Write(argument.Name);
                output.Write(">");
            }

            if (application.AllowArgumentSeparator)
            {
                output.Write(" [[--] <arg>...]");
            }

            output.WriteLine();
        }

        /// <summary>
        /// Generate the lines that show information about arguments
        /// </summary>
        /// <param name="application">The app</param>
        /// <param name="output">Help text output</param>
        /// <param name="visibleArguments">Arguments not hidden from help text</param>
        /// <param name="firstColumnWidth">The width of the first column of commands, arguments, and options</param>
        protected virtual void GenerateArguments(
            CommandLineApplication application,
            TextWriter output,
            IReadOnlyList<CommandArgument> visibleArguments,
            int firstColumnWidth)
        {
            if (visibleArguments.Any())
            {
                output.WriteLine();
                output.WriteLine("Arguments:");
                var outputFormat = $"  {{0, -{firstColumnWidth}}}{{1}}";

                foreach (var arg in visibleArguments)
                {
                    var description = arg.Description;
                    var allowedValuesBeenSet = false;

                    foreach (var attributeValidator in arg.Validators.Cast<AttributeValidator>())
                    {
                        if (attributeValidator?.ValidationAttribute is AllowedValuesAttribute allowedValuesAttribute)
                        {
                            description += $"\nAllowed values are: {string.Join(", ", allowedValuesAttribute.AllowedValues)}.";
                            allowedValuesBeenSet = true;
                            break;
                        }
                    }

                    if (!allowedValuesBeenSet)
                    {
                        var enumNames = ExtractNamesFromEnum(arg.UnderlyingType);
                        if (enumNames.Any())
                        {
                            description += $"\nAllowed values are: {string.Join(", ", enumNames)}.";
                        }
                    }

                    var wrappedDescription = IndentWriter?.Write(description);
                    var message = string.Format(outputFormat, arg.Name, wrappedDescription);

                    output.Write(message);
                    output.WriteLine();
                }
            }
        }

        /// <summary>
        /// Generate the lines that show information about options
        /// </summary>
        /// <param name="application">The app</param>
        /// <param name="output">Help text output</param>
        /// <param name="visibleOptions">Options not hidden from help text</param>
        /// <param name="firstColumnWidth">The width of the first column of commands, arguments, and options</param>
        protected virtual void GenerateOptions(
            CommandLineApplication application,
            TextWriter output,
            IReadOnlyList<CommandOption> visibleOptions,
            int firstColumnWidth)
        {
            if (visibleOptions.Any())
            {
                output.WriteLine();
                output.WriteLine("Options:");
                var outputFormat = $"  {{0, -{firstColumnWidth}}}{{1}}";

                foreach (var opt in visibleOptions)
                {
                    var description = opt.Description;
                    var allowedValuesBeenSet = false;

                    foreach (var attributeValidator in opt.Validators.Cast<AttributeValidator>())
                    {
                        if (attributeValidator?.ValidationAttribute is AllowedValuesAttribute allowedValuesAttribute)
                        {
                            description += $"\nAllowed values are: {string.Join(", ", allowedValuesAttribute.AllowedValues)}.";
                            allowedValuesBeenSet = true;
                            break;
                        }
                    }

                    if (!allowedValuesBeenSet)
                    {
                        var enumNames = ExtractNamesFromEnum(opt.UnderlyingType);
                        if (enumNames.Any())
                        {
                            description += $"\nAllowed values are: {string.Join(", ", enumNames)}.";
                        }
                    }

                    var wrappedDescription = IndentWriter?.Write(description);
                    var message = string.Format(outputFormat, Format(opt), wrappedDescription);

                    output.Write(message);
                    output.WriteLine();
                }
            }
        }


        /// <summary>
        /// Generate the lines that show information about subcommands
        /// </summary>
        /// <param name="application">The app</param>
        /// <param name="output">Help text output</param>
        /// <param name="visibleCommands">Commands not hidden from help text</param>
        /// <param name="firstColumnWidth">The width of the first column of commands, arguments, and options</param>
        protected virtual void GenerateCommands(
            CommandLineApplication application,
            TextWriter output,
            IReadOnlyList<CommandLineApplication> visibleCommands,
            int firstColumnWidth)
        {
            if (visibleCommands.Any())
            {
                output.WriteLine();
                output.WriteLine("Commands:");
                var outputFormat = $"  {{0, -{firstColumnWidth}}}{{1}}";

                var orderedCommands = SortCommandsByName
                    ? visibleCommands.OrderBy(c => c.Name).ToList()
                    : visibleCommands;

                foreach (var cmd in orderedCommands)
                {
                    var description = cmd.Description;

                    var wrappedDescription = IndentWriter?.Write(description);
                    var message = string.Format(outputFormat, cmd.Name, wrappedDescription);

                    output.Write(message);
                    output.WriteLine();
                }

                if (application.OptionHelp != null)
                {
                    output.WriteLine();
                    output.WriteLine($"Run '{application.Name} [command] {Format(application.OptionHelp)}' for more information about a command.");
                }
            }
        }

        /// <summary>
        /// Generate the last lines of help text output
        /// </summary>
        /// <param name="application">The app</param>
        /// <param name="output">Help text output</param>
        protected virtual void GenerateFooter(
            CommandLineApplication application,
            TextWriter output)
        {
            output.Write(application.ExtendedHelpText);
            output.WriteLine();
        }

        /// <summary>
        /// Generates the template string in the format "-{Symbol}|-{Short}|--{Long} &lt;{Value}&gt;" for display in help text.
        /// </summary>
        /// <returns>The template string</returns>
        protected virtual string Format(CommandOption option)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(option.SymbolName))
            {
                sb.Append('-').Append(option.SymbolName);
            }

            if (!string.IsNullOrEmpty(option.ShortName))
            {
                if (sb.Length > 0)
                {
                    sb.Append('|');
                }

                sb.Append('-').Append(option.ShortName);
            }

            if (!string.IsNullOrEmpty(option.LongName))
            {
                if (sb.Length > 0)
                {
                    sb.Append('|');
                }

                sb.Append("--").Append(option.LongName);
            }

            if (!string.IsNullOrEmpty(option.ValueName) && option.OptionType != CommandOptionType.NoValue)
            {
                if (option.OptionType == CommandOptionType.SingleOrNoValue)
                {
                    sb.Append("[:<").Append(option.ValueName).Append(">]");
                }
                else
                {
                    sb.Append(" <").Append(option.ValueName).Append('>');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get the Console width.
        /// </summary>
        /// <returns>BufferWidth or the default.</returns>
        private int? TryGetConsoleWidth()
        {
            try
            {
                var retVal = MaxLineLength ?? Console.BufferWidth;

                return retVal > 0
                    ? retVal
                    : default(int?);
            }
            catch (IOException)
            {
                // If there isn't a console - for instance in test environments
                // An IOException will be thrown trying to get the Console.BufferWidth.
                return null;
            }
        }

        private string[] ExtractNamesFromEnum(Type type)
        {
            if (type == null)
            {
                return Util.EmptyArray<string>();
            }

            if (ReflectionHelper.IsNullableType(type, out var wrappedType))
            {
                return ExtractNamesFromEnum(wrappedType);
            }

            if (ReflectionHelper.IsSpecialValueTupleType(type, out var fieldInfo))
            {
                return ExtractNamesFromEnum(fieldInfo.FieldType);
            }

            if (type.IsEnum)
            {
                return Enum.GetNames(type);
            }

            return Util.EmptyArray<string>();
        }
    }
}
