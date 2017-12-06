// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Describes a set of command line arguments, options, and execution behavior.
    /// <see cref="CommandLineApplication"/> can be nested to support subcommands.
    /// </summary>
    public partial class CommandLineApplication
    {
        private IConsole _console;

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="ThrowOnUnexpectedArgument"/>.</param>
        public CommandLineApplication(bool throwOnUnexpectedArg = true)
            : this(PhysicalConsole.Singleton, Directory.GetCurrentDirectory(), throwOnUnexpectedArg)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        public CommandLineApplication(IConsole console)
            : this(console, Directory.GetCurrentDirectory(), throwOnUnexpectedArg: true)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        /// <param name="throwOnUnexpectedArg">Initial value for <see cref="ThrowOnUnexpectedArgument"/>.</param>
        public CommandLineApplication(IConsole console, string workingDirectory, bool throwOnUnexpectedArg)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (string.IsNullOrEmpty(workingDirectory))
            {
                throw new ArgumentException("Argument must not be null or empty", nameof(workingDirectory));
            }

            ThrowOnUnexpectedArgument = throwOnUnexpectedArg;
            WorkingDirectory = workingDirectory;
            Options = new List<CommandOption>();
            Arguments = new List<CommandArgument>();
            Commands = new List<CommandLineApplication>();
            RemainingArguments = new List<string>();
            Invoke = () => 0;
            ValidationErrorHandler = DefaultValidationErrorHandler;
            SetConsole(console);
        }

        private CommandLineApplication(CommandLineApplication parent, string name, bool throwOnUnexpectedArg)
            : this(parent._console, parent.WorkingDirectory, throwOnUnexpectedArg)
        {
            Name = name;
            Parent = parent;
            StopParsingAfterHelpOption = parent.StopParsingAfterHelpOption;
            StopParsingAfterVersionOption = parent.StopParsingAfterVersionOption;
        }

        /// <summary>
        /// Defaults to null. A link to the parent command if this is instance is a subcommand.
        /// </summary>
        public CommandLineApplication Parent { get; set; }

        /// <summary>
        /// The short name of the command. When this is a subcommand, it is the name of the word used to invoke the subcommand.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The full name of the command to show in the help text.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// A description of the command.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines if this command appears in generated help text.
        /// </summary>
        public bool ShowInHelpText { get; set; } = true;

        /// <summary>
        /// Additional text that appears at the bottom of generated help text.
        /// </summary>
        public string ExtendedHelpText { get; set; }

        /// <summary>
        /// Available command-line options on this command. Use <see cref="GetOptions"/> to get all available options, which may include inherited options.
        /// </summary>
        public List<CommandOption> Options { get; private set; }

        /// <summary>
        /// The option used to determine if help text should be displayed. This is set by calling <see cref="HelpOption(string)"/>.
        /// </summary>
        public CommandOption OptionHelp { get; internal set; }

        /// <summary>
        /// The options used to determine if the command version should be displayed. This is set by calling <see cref="VersionOption(string, Func{string}, Func{string})"/>.
        /// </summary>
        public CommandOption OptionVersion { get; internal set; }

        /// <summary>
        /// Required command-line arguments.
        /// </summary>
        public List<CommandArgument> Arguments { get; private set; }

        /// <summary>
        /// When initialized with <see cref="ThrowOnUnexpectedArgument"/> to <c>false</c>, this will contain any unrecognized arguments.
        /// </summary>
        public List<string> RemainingArguments { get; private set; }

        /// <summary>
        /// Indicates whether the parser should throw an exception when it runs into an unexpected argument.
        /// If this field is set to false, the parser will stop parsing when it sees an unexpected argument, and all
        /// remaining arguments, including the first unexpected argument, will be stored in RemainingArguments property.
        /// </summary>
        public bool ThrowOnUnexpectedArgument { get; set; }

        /// <summary>
        /// True when <see cref="OptionHelp"/> or <see cref="OptionVersion"/> was matched.
        /// </summary>
        public bool IsShowingInformation { get; protected set; }

        /// <summary>
        /// Stops the parsing argument when <see cref="OptionHelp"/> is matched. Defaults to <c>true</c>.
        /// This will prevent <see cref="Invoke" /> or <see cref="ValidationErrorHandler" /> from being called.
        /// </summary>
        public bool StopParsingAfterHelpOption { get; set; } = true;

        /// <summary>
        /// Stops the parsing argument when <see cref="OptionVersion"/> is matched. Defaults to <c>true</c>.
        /// This will prevent <see cref="Invoke" /> or <see cref="ValidationErrorHandler" /> from being called.
        /// </summary>
        public bool StopParsingAfterVersionOption { get; set; } = true;

        /// <summary>
        /// The action to call when this command is matched and <see cref="IsShowingInformation"/> is <c>false</c>.
        /// </summary>
        public Func<int> Invoke { get; set; }

        /// <summary>
        /// The long-form of the version to display in generated help text.
        /// </summary>
        public Func<string> LongVersionGetter { get; set; }
        /// <summary>
        /// The short-form of the version to display in generated help text.
        /// </summary>
        public Func<string> ShortVersionGetter { get; set; }

        /// <summary>
        /// Subcommands.
        /// </summary>
        public List<CommandLineApplication> Commands { get; private set; }

        /// <summary>
        /// Determines if '--' can be used to separate known arguments and options from additional content passed to <see cref="RemainingArguments"/>.
        /// </summary>
        public bool AllowArgumentSeparator { get; set; }

        /// <summary>
        /// <para>
        /// When enabled, the parser will treat any arguments beginning with '@' as a file path to a response file.
        /// A response file contains additional arguments that will be treated as if they were passed in on the command line.
        /// </para>
        /// <para>
        /// Defaults to <see cref="ResponseFileHandling.Disabled" />.
        /// </para>
        /// <para>
        /// Nested response false are not supported.
        /// </para>
        /// </summary>
        public ResponseFileHandling ResponseFileHandling { get; set; }

        /// <summary>
        /// <para>
        /// Defines the working directory of the application. Defaults to <see cref="Directory.GetCurrentDirectory"/>.
        /// </para>
        /// <para>
        /// This will be used as the base path for opening response files when <see cref="ResponseFileHandling"/> is <c>true</c>.
        /// </para>
        /// </summary>
        public string WorkingDirectory { get; }

        /// <summary>
        /// The writer used to display generated help text.
        /// </summary>
        public TextWriter Out { get; set; }

        /// <summary>
        /// The writer used to display generated error messages.
        /// </summary>
        public TextWriter Error { get; set; }

        /// <summary>
        /// Gets all command line options available to this command, including any inherited options.
        /// </summary>
        /// <returns>Command line options.</returns>
        public IEnumerable<CommandOption> GetOptions()
        {
            var expr = Options.AsEnumerable();
            var rootNode = this;
            while (rootNode.Parent != null)
            {
                rootNode = rootNode.Parent;
                expr = expr.Concat(rootNode.Options.Where(o => o.Inherited));
            }

            return expr;
        }

        /// <summary>
        /// Adds a subcommand.
        /// </summary>
        /// <param name="name">The word used to invoke the subcommand.</param>
        /// <param name="configuration"></param>
        /// <param name="throwOnUnexpectedArg"></param>
        /// <returns></returns>
        public CommandLineApplication Command(string name, Action<CommandLineApplication> configuration,
            bool throwOnUnexpectedArg = true)
        {
            var command = new CommandLineApplication(this, name, throwOnUnexpectedArg);

            Commands.Add(command);
            configuration(command);
            return command;
        }

        /// <summary>
        /// Adds a command-line option.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="description"></param>
        /// <param name="optionType"></param>
        /// <returns></returns>
        public CommandOption Option(string template, string description, CommandOptionType optionType)
            => Option(template, description, optionType, _ => { }, inherited: false);

        /// <summary>
        /// Adds a command-line option.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="description"></param>
        /// <param name="optionType"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        public CommandOption Option(string template, string description, CommandOptionType optionType, bool inherited)
            => Option(template, description, optionType, _ => { }, inherited);

        /// <summary>
        /// Adds a command-line option.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="description"></param>
        /// <param name="optionType"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public CommandOption Option(string template, string description, CommandOptionType optionType, Action<CommandOption> configuration)
            => Option(template, description, optionType, configuration, inherited: false);

        /// <summary>
        /// Adds a command line options.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="description"></param>
        /// <param name="optionType"></param>
        /// <param name="configuration"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        public CommandOption Option(string template, string description, CommandOptionType optionType, Action<CommandOption> configuration, bool inherited)
        {
            var option = new CommandOption(template, optionType)
            {
                Description = description,
                Inherited = inherited
            };
            Options.Add(option);
            configuration(option);
            return option;
        }

        /// <summary>
        /// Adds a command line argument
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="multipleValues"></param>
        /// <returns></returns>
        public CommandArgument Argument(string name, string description, bool multipleValues = false)
        {
            return Argument(name, description, _ => { }, multipleValues);
        }

        /// <summary>
        /// Adds a command line argument.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="configuration"></param>
        /// <param name="multipleValues"></param>
        /// <returns></returns>
        public CommandArgument Argument(string name, string description, Action<CommandArgument> configuration, bool multipleValues = false)
        {
            var lastArg = Arguments.LastOrDefault();
            if (lastArg != null && lastArg.MultipleValues)
            {
                throw new InvalidOperationException(Strings.OnlyLastArgumentCanAllowMultipleValues(lastArg.Name));
            }

            var argument = new CommandArgument
            {
                Name = name,
                Description = description,
                MultipleValues = multipleValues
            };
            Arguments.Add(argument);
            configuration(argument);
            return argument;
        }

        /// <summary>
        /// Defines a callback for when this command is invoked.
        /// </summary>
        /// <param name="invoke"></param>
        public void OnExecute(Func<int> invoke)
        {
            Invoke = invoke;
        }

        /// <summary>
        /// Defines an asynchronous callback.
        /// </summary>
        /// <param name="invoke"></param>
        public void OnExecute(Func<Task<int>> invoke)
        {
            Invoke = () => invoke().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Parses an array of strings, matching them against <see cref="Options"/>, <see cref="Arguments"/>, and <see cref="Commands"/>.
        /// If this command is matched, it will invoke <see cref="Invoke"/>.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>The return code from <see cref="Invoke"/>.</returns>
        public int Execute(params string[] args)
        {
            args = args ?? new string[0];

            var processor = new CommandLineProcessor(this, args);
            var command = processor.Process();

            var result = command.GetValidationResult();

            if (result != ValidationResult.Success)
            {
                return command.ValidationErrorHandler(result);
            }

            return command.Invoke();
        }

        /// <summary>
        /// Helper method that adds a help option.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public CommandOption HelpOption(string template)
        {
            // Help option is special because we stop parsing once we see it
            // So we store it separately for further use
            OptionHelp = Option(template, Strings.DefaultHelpOptionDescription, CommandOptionType.NoValue);

            return OptionHelp;
        }

        /// <summary>
        /// Helper method that adds a version option from known versions strings.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="shortFormVersion"></param>
        /// <param name="longFormVersion"></param>
        /// <returns></returns>
        public CommandOption VersionOption(string template,
            string shortFormVersion,
            string longFormVersion = null)
        {
            if (longFormVersion == null)
            {
                return VersionOption(template, () => shortFormVersion);
            }
            else
            {
                return VersionOption(template, () => shortFormVersion, () => longFormVersion);
            }
        }

        /// <summary>
        /// Helper method that adds a version option.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="shortFormVersionGetter"></param>
        /// <param name="longFormVersionGetter"></param>
        /// <returns></returns>
        public CommandOption VersionOption(string template,
            Func<string> shortFormVersionGetter,
            Func<string> longFormVersionGetter = null)
        {
            // Version option is special because we stop parsing once we see it
            // So we store it separately for further use
            OptionVersion = Option(template, Strings.DefaultVersionOptionDescription, CommandOptionType.NoValue);
            ShortVersionGetter = shortFormVersionGetter;
            LongVersionGetter = longFormVersionGetter ?? shortFormVersionGetter;

            return OptionVersion;
        }

        /// <summary>
        /// Show short hint that reminds users to use help option.
        /// </summary>
        public void ShowHint()
        {
            if (OptionHelp != null)
            {
                Out.WriteLine(string.Format("Specify --{0} for a list of available options and commands.", OptionHelp.LongName));
            }
        }

        /// <summary>
        /// Show full help.
        /// </summary>
        /// <param name="commandName">The subcommand for which to show help. Leave null to show for the current command.</param>
        public void ShowHelp(string commandName = null)
        {
            for (var cmd = this; cmd != null; cmd = cmd.Parent)
            {
                cmd.IsShowingInformation = true;
            }

            Out.WriteLine(GetHelpText(commandName));
        }

        /// <summary>
        /// Produces help text describing command usage.
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        public virtual string GetHelpText(string commandName = null)
        {
            var headerBuilder = new StringBuilder("Usage:");
            for (var cmd = this; cmd != null; cmd = cmd.Parent)
            {
                headerBuilder.Insert(6, string.Format(" {0}", cmd.Name));
            }

            CommandLineApplication target;

            if (commandName == null || string.Equals(Name, commandName, StringComparison.OrdinalIgnoreCase))
            {
                target = this;
            }
            else
            {
                target = Commands.SingleOrDefault(cmd => string.Equals(cmd.Name, commandName, StringComparison.OrdinalIgnoreCase));

                if (target != null)
                {
                    headerBuilder.AppendFormat(" {0}", commandName);
                }
                else
                {
                    // The command name is invalid so don't try to show help for something that doesn't exist
                    target = this;
                }

            }

            var optionsBuilder = new StringBuilder();
            var commandsBuilder = new StringBuilder();
            var argumentsBuilder = new StringBuilder();

            var arguments = target.Arguments.Where(a => a.ShowInHelpText).ToList();
            if (arguments.Any())
            {
                headerBuilder.Append(" [arguments]");

                argumentsBuilder.AppendLine();
                argumentsBuilder.AppendLine("Arguments:");
                var maxArgLen = arguments.Max(a => a.Name.Length);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxArgLen + 2);
                foreach (var arg in arguments)
                {
                    argumentsBuilder.AppendFormat(outputFormat, arg.Name, arg.Description);
                    argumentsBuilder.AppendLine();
                }
            }

            var options = target.GetOptions().Where(o => o.ShowInHelpText).ToList();
            if (options.Any())
            {
                headerBuilder.Append(" [options]");

                optionsBuilder.AppendLine();
                optionsBuilder.AppendLine("Options:");
                var maxOptLen = options.Max(o => o.Template?.Length ?? 0);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxOptLen + 2);
                foreach (var opt in options)
                {
                    optionsBuilder.AppendFormat(outputFormat, opt.Template, opt.Description);
                    optionsBuilder.AppendLine();
                }
            }

            var commands = target.Commands.Where(c => c.ShowInHelpText).ToList();
            if (commands.Any())
            {
                headerBuilder.Append(" [command]");

                commandsBuilder.AppendLine();
                commandsBuilder.AppendLine("Commands:");
                var maxCmdLen = commands.Max(c => c.Name?.Length ?? 0);
                var outputFormat = string.Format("  {{0, -{0}}}{{1}}", maxCmdLen + 2);
                foreach (var cmd in commands.OrderBy(c => c.Name))
                {
                    commandsBuilder.AppendFormat(outputFormat, cmd.Name, cmd.Description);
                    commandsBuilder.AppendLine();
                }

                if (OptionHelp != null)
                {
                    commandsBuilder.AppendLine();
                    commandsBuilder.AppendFormat($"Use \"{target.Name} [command] --{OptionHelp.LongName}\" for more information about a command.");
                    commandsBuilder.AppendLine();
                }
            }

            if (target.AllowArgumentSeparator)
            {
                headerBuilder.Append(" [[--] <arg>...]");
            }

            headerBuilder.AppendLine();

            var nameAndVersion = new StringBuilder();
            nameAndVersion.AppendLine(GetFullNameAndVersion());
            nameAndVersion.AppendLine();

            return nameAndVersion.ToString()
                + headerBuilder.ToString()
                + argumentsBuilder.ToString()
                + optionsBuilder.ToString()
                + commandsBuilder.ToString()
                + target.ExtendedHelpText;
        }

        /// <summary>
        /// Displays version information that includes <see cref="FullName"/> and <see cref="LongVersionGetter"/>.
        /// </summary>
        public void ShowVersion()
        {
            for (var cmd = this; cmd != null; cmd = cmd.Parent)
            {
                cmd.IsShowingInformation = true;
            }

            if (!string.IsNullOrEmpty(FullName))
            {
                Out.WriteLine(FullName);
            }

            Out.WriteLine(LongVersionGetter());
        }

        /// <summary>
        /// Gets <see cref="FullName"/> and <see cref="ShortVersionGetter"/>.
        /// </summary>
        /// <returns></returns>
        public string GetFullNameAndVersion()
        {
            return ShortVersionGetter == null ? FullName : string.Format("{0} {1}", FullName, ShortVersionGetter());
        }

        /// <summary>
        /// Traverses up <see cref="Parent"/> and displays the result of <see cref="GetFullNameAndVersion"/>.
        /// </summary>
        public void ShowRootCommandFullNameAndVersion()
        {
            var rootCmd = this;
            while (rootCmd.Parent != null)
            {
                rootCmd = rootCmd.Parent;
            }

            Out.WriteLine(rootCmd.GetFullNameAndVersion());
            Out.WriteLine();
        }

        private bool _settingConsole;

        internal void SetConsole(IConsole console)
        {
            if (_settingConsole)
            {
                // prevent stack overflow in the event someone has looping command line apps
                return;
            }

            _settingConsole = true;
            _console = console;
            Out = console.Out;
            Error = console.Error;

            foreach (var cmd in Commands)
            {
                cmd.SetConsole(console);
            }

            _settingConsole = false;
        }
    }
}
