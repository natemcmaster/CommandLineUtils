// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Conventions;
using McMaster.Extensions.CommandLineUtils.HelpText;
using McMaster.Extensions.CommandLineUtils.Internal;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Describes a set of command line arguments, options, and execution behavior.
    /// <see cref="CommandLineApplication"/> can be nested to support subcommands.
    /// </summary>
    public partial class CommandLineApplication : IServiceProvider, IDisposable
    {
        private const int HelpExitCode = 0;
        internal const int ValidationErrorExitCode = 1;
        private static readonly int s_exitCodeOperationCanceled;

        static CommandLineApplication()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // values from https://www.febooti.com/products/automation-workshop/online-help/events/run-dos-cmd-command/exit-codes/
                s_exitCodeOperationCanceled = unchecked((int)0xC000013A);
            }
            else
            {
                // Match Process.ExitCode which uses 128 + signo.
                s_exitCodeOperationCanceled = 130; // SIGINT
            }
        }

        private static Task<int> DefaultAction(CancellationToken ct) => Task.FromResult(0);
        private Func<CancellationToken, Task<int>> _handler;
        private List<Action<ParseResult>>? _onParsingComplete;
        internal readonly Dictionary<string, PropertyInfo> _shortOptions = new();
        internal readonly Dictionary<string, PropertyInfo> _longOptions = new();
        private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);
        private string? _primaryCommandName;
        internal CommandLineContext _context;
        private readonly ParserConfig _parserConfig;
        private IHelpTextGenerator _helpTextGenerator;
        private CommandOption? _optionHelp;
        private readonly Lazy<IServiceProvider> _services;
        private readonly ConventionContext _conventionContext;
        private readonly List<IConvention> _conventions = new();
        private readonly List<CommandArgument> _arguments = new();
        private readonly List<CommandOption> _options = new();
        private readonly List<CommandLineApplication> _subcommands = new();
        internal readonly List<string> _remainingArguments = new();

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        public CommandLineApplication()
            : this(null, DefaultHelpTextGenerator.Singleton, new DefaultCommandLineContext())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        public CommandLineApplication(IConsole console)
            : this(null, DefaultHelpTextGenerator.Singleton, new DefaultCommandLineContext(console))
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        public CommandLineApplication(IConsole console, string workingDirectory)
            : this(null, DefaultHelpTextGenerator.Singleton, new DefaultCommandLineContext(console, workingDirectory))
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="CommandLineApplication"/>.
        /// </summary>
        /// <param name="helpTextGenerator">The help text generator to use.</param>
        /// <param name="console">The console implementation to use.</param>
        /// <param name="workingDirectory">The current working directory.</param>
        public CommandLineApplication(IHelpTextGenerator helpTextGenerator, IConsole console, string workingDirectory)
            : this(null, helpTextGenerator, new DefaultCommandLineContext(console, workingDirectory))
        {
        }

        internal CommandLineApplication(CommandLineApplication parent, string name)
            : this(parent, parent._helpTextGenerator, parent._context)
        {
            if (name != null)
            {
                Name = name;
            }
        }

        internal CommandLineApplication(
            CommandLineApplication? parent,
            IHelpTextGenerator helpTextGenerator,
            CommandLineContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Parent = parent;
            _helpTextGenerator = helpTextGenerator ?? throw new ArgumentNullException(nameof(helpTextGenerator));
            _handler = DefaultAction;
            _validationErrorHandler = DefaultValidationErrorHandler;
            Out = context.Console.Out;
            Error = context.Console.Error;
            SetContext(context);
            _services = new Lazy<IServiceProvider>(() => new ServiceProvider(this));
            ValueParsers = parent?.ValueParsers ?? new ValueParserProvider();
            _parserConfig = new ParserConfig();
            _clusterOptions = parent?._clusterOptions;
            UsePagerForHelpText = parent?.UsePagerForHelpText ?? false;

            _conventionContext = CreateConventionContext();

            if (Parent != null)
            {
                foreach (var convention in Parent._conventions)
                {
                    Conventions.AddConvention(convention);
                }
            }
        }

        /// <summary>
        /// Defaults to null. A link to the parent command if this is instance is a subcommand.
        /// </summary>
        public CommandLineApplication? Parent { get; set; }

        /// <summary>
        /// The help text generator to use.
        /// </summary>
        public IHelpTextGenerator HelpTextGenerator
        {
            get => _helpTextGenerator;
            set => _helpTextGenerator = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The short name of the command. When this is a subcommand, it is the name of the word used to invoke the subcommand.
        /// </summary>
        public string? Name
        {
            get => _primaryCommandName;
            set
            {
                Parent?.AssertCommandNameIsUnique(value, this);
                _primaryCommandName = value;
            }
        }

        /// <summary>
        /// The full name of the command to show in the help text.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// A description of the command.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Determines if this command appears in generated help text.
        /// </summary>
        public bool ShowInHelpText { get; set; } = true;

        /// <summary>
        /// Additional text that appears at the bottom of generated help text.
        /// </summary>
        public string? ExtendedHelpText { get; set; }

        /// <summary>
        /// Available command-line options on this command. Use <see cref="GetOptions"/> to get all available options, which may include inherited options.
        /// </summary>
        public IReadOnlyCollection<CommandOption> Options => _options;

        /// <summary>
        /// Whether a Pager should be used to display help text.
        /// </summary>
        public bool UsePagerForHelpText { get; set; }

        /// <summary>
        /// All names by which the command can be referenced. This includes <see cref="Name"/> and an aliases added in <see cref="AddName"/>.
        /// </summary>
        public IEnumerable<string> Names
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    yield return Name;
                }

                foreach (var names in _names)
                {
                    yield return names;
                }
            }
        }

        /// <summary>
        /// The option used to determine if help text should be displayed. This is set by calling <see cref="HelpOption(string)"/>.
        /// </summary>
        public CommandOption? OptionHelp
        {
            get
            {
                if (_optionHelp != null)
                {
                    return _optionHelp;
                }
                return Parent?.OptionHelp?.Inherited == true
                    ? Parent.OptionHelp
                    : null;
            }
            internal set => _optionHelp = value;
        }


        /// <summary>
        /// The options used to determine if the command version should be displayed. This is set by calling <see cref="VersionOption(string, Func{string}, Func{string})"/>.
        /// </summary>
        public CommandOption? OptionVersion { get; internal set; }

        /// <summary>
        /// Required command-line arguments.
        /// </summary>
        public IReadOnlyList<CommandArgument> Arguments => _arguments;

        /// <summary>
        /// When initialized when <see cref="UnrecognizedArgumentHandling"/> is <see cref="McMaster.Extensions.CommandLineUtils.UnrecognizedArgumentHandling.StopParsingAndCollect" />,
        /// this will contain any unrecognized arguments.
        /// </summary>
        public IReadOnlyList<string> RemainingArguments => _remainingArguments;

        /// <summary>
        /// Configures what the parser should do when it runs into an unexpected argument.
        /// </summary>
        public UnrecognizedArgumentHandling UnrecognizedArgumentHandling
        {
            get => _parserConfig.UnrecognizedArgumentHandling ?? Parent?.UnrecognizedArgumentHandling ?? UnrecognizedArgumentHandling.Throw;
            set => _parserConfig.UnrecognizedArgumentHandling = value;
        }

        /// <summary>
        /// True when <see cref="OptionHelp"/> or <see cref="OptionVersion"/> was matched.
        /// </summary>
        public bool IsShowingInformation { get; protected set; }

        /// <summary>
        /// The long-form of the version to display in generated help text.
        /// </summary>
        public Func<string?>? LongVersionGetter { get; set; }

        /// <summary>
        /// The short-form of the version to display in generated help text.
        /// </summary>
        public Func<string?>? ShortVersionGetter { get; set; }

        /// <summary>
        /// Subcommands.
        /// </summary>
        public IReadOnlyCollection<CommandLineApplication> Commands => _subcommands;

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
        /// Defaults to <see cref="McMaster.Extensions.CommandLineUtils.ResponseFileHandling.Disabled" />.
        /// </para>
        /// <para>
        /// Nested response false are not supported.
        /// </para>
        /// </summary>
        public ResponseFileHandling ResponseFileHandling { get; set; }

        /// <summary>
        /// The way arguments and options are matched.
        /// </summary>
        public StringComparison OptionsComparison { get; set; }

        /// <summary>
        /// <para>
        /// One or more options of <see cref="CommandOptionType.NoValue"/>, followed by at most one option that takes values, should be accepted when grouped behind one '-' delimiter.
        /// </para>
        /// <para>
        /// When true, the following are equivalent.
        ///
        /// <code>
        /// -abcXyellow
        /// -abcX=yellow
        /// -abcX:yellow
        /// -abc -X=yellow
        /// -ab -cX=yellow
        /// -a -b -c -Xyellow
        /// -a -b -c -X yellow
        /// -a -b -c -X=yellow
        /// -a -b -c -X:yellow
        /// </code>
        /// </para>
        /// <para>
        /// This defaults to true unless an option with a short name of two or more characters is added.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <seealso href="https://www.gnu.org/software/libc/manual/html_node/Argument-Syntax.html"/>
        /// </remarks>
        public bool ClusterOptions
        {
            // unless explicitly set, use the value of cluster options from the parent command
            // or default to true if this is the root command
            get => _clusterOptions ?? (Parent == null || Parent.ClusterOptions);
            set => _clusterOptions = value;
        }

        private bool? _clusterOptions;

        internal bool ClusterOptionsWasSetExplicitly => _clusterOptions.HasValue;

        /// <summary>
        /// Characters used to separate the option name from the value.
        /// <para>
        /// By default, allowed separators are ' ' (space), :, and =
        /// </para>
        /// </summary>
        /// <remarks>
        /// Space actually implies multiple spaces due to the way most operating system shells parse command
        /// line arguments before starting a new process.
        /// </remarks>
        /// <example>
        /// Given --name=value, = is the separator.
        /// </example>
        public char[] OptionNameValueSeparators
        {
            get => _parserConfig.OptionNameValueSeparators ?? Parent?.OptionNameValueSeparators ?? new[] { ' ', ':', '=' };
            set
            {
                if (value.Length == 0)
                {
                    throw new ArgumentException(Strings.IsEmptyArray, nameof(value));
                }

                _parserConfig.OptionNameValueSeparators = value;
            }
        }

        internal bool OptionNameAndValueCanBeSpaceSeparated => Array.IndexOf(OptionNameValueSeparators, ' ') >= 0;

        /// <summary>
        /// Gets the default value parser provider.
        /// <para>
        /// The value parsers control how argument values are converted from strings to other types. Additional value
        /// parsers can be added so that domain specific types can converted. In-built value parsers can also be replaced
        /// for precise control of all type conversion.
        /// </para>
        /// <remarks>
        /// Value parsers are currently only used by the Attribute API.
        /// </remarks>
        /// </summary>
        public ValueParserProvider ValueParsers { get; private set; }

        /// <summary>
        /// <para>
        /// Defines the working directory of the application. Defaults to <see cref="Directory.GetCurrentDirectory"/>.
        /// </para>
        /// <para>
        /// This will be used as the base path for opening response files when <see cref="ResponseFileHandling"/> is <c>true</c>.
        /// </para>
        /// </summary>
        public string WorkingDirectory => _context.WorkingDirectory;

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
        /// Add another name for the command.
        /// <para>
        /// Additional names can be shorter, longer, or alternative names by which a command may be invoked on the command line.
        /// </para>
        /// </summary>
        /// <param name="name">The name. Must not be null or empty.</param>
        public void AddName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            Parent?.AssertCommandNameIsUnique(name, this);

            _names.Add(name);
        }

        /// <summary>
        /// Add a subcommand
        /// </summary>
        /// <param name="subcommand"></param>
        public void AddSubcommand(CommandLineApplication subcommand)
        {
            if (subcommand == null)
            {
                throw new ArgumentNullException(nameof(subcommand));
            }

            foreach (var name in subcommand.Names)
            {
                AssertCommandNameIsUnique(name, null);
            }

            subcommand.Parent = this;

            _subcommands.Add(subcommand);
        }

        private void AssertCommandNameIsUnique(string? name, CommandLineApplication? commandToIgnore)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            foreach (var cmd in Commands)
            {
                if (ReferenceEquals(cmd, commandToIgnore))
                {
                    continue;
                }

                if (cmd.MatchesName(name))
                {
                    throw new InvalidOperationException(Strings.DuplicateSubcommandName(name));
                }
            }
        }

        /// <summary>
        /// Adds a subcommand.
        /// </summary>
        /// <param name="name">The word used to invoke the subcommand.</param>
        /// <param name="configuration">A callback to configure the created subcommand.</param>
        /// <returns></returns>
        public CommandLineApplication Command(string name, Action<CommandLineApplication> configuration)
        {
            var command = new CommandLineApplication(this, name);

            AddSubcommand(command);

            configuration?.Invoke(command);

            return command;
        }

        /// <summary>
        /// Adds a subcommand with model of type <typeparamref name="TModel" />.
        /// </summary>
        /// <param name="name">The word used to invoke the subcommand.</param>
        /// <param name="configuration">A callback used to configure the subcommand object.</param>
        /// <typeparam name="TModel">The model type of the subcommand.</typeparam>
        /// <returns></returns>
        public CommandLineApplication<TModel> Command<TModel>(string name, Action<CommandLineApplication<TModel>> configuration)
            where TModel : class
        {
            var command = new CommandLineApplication<TModel>(this, name);

            AddSubcommand(command);

            configuration?.Invoke(command);

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
        /// Adds a command line option.
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
            AddOption(option);
            configuration(option);
            return option;
        }

        /// <summary>
        /// Add an option to the definition of this command
        /// </summary>
        /// <param name="option"></param>
        public void AddOption(CommandOption option)
        {
            _options.Add(option);
        }

        /// <summary>
        /// Adds a command line option with values that should be parsable into <typeparamref name="T" />.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="description"></param>
        /// <param name="optionType"></param>
        /// <param name="configuration"></param>
        /// <param name="inherited"></param>
        /// <typeparam name="T">The type of the values on the option</typeparam>
        /// <returns>The option</returns>
        public CommandOption<T> Option<T>(string template, string description, CommandOptionType optionType, Action<CommandOption<T>> configuration, bool inherited)
        {
            var parser = ValueParsers.GetParser<T>();

            if (parser == null)
            {
                throw new InvalidOperationException(Strings.CannotDetermineParserType(typeof(T)));
            }

            var option = new CommandOption<T>(parser, template, optionType)
            {
                Description = description,
                Inherited = inherited
            };
            AddOption(option);
            configuration(option);
            return option;
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Adds a command line argument
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="multipleValues"></param>
        /// <returns></returns>
        public CommandArgument Argument(string name, string description, bool multipleValues = false)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            => Argument(name, description, _ => { }, multipleValues);

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Adds a command line argument.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="configuration"></param>
        /// <param name="multipleValues"></param>
        /// <returns></returns>
        public CommandArgument Argument(string name, string description, Action<CommandArgument> configuration, bool multipleValues = false)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            var argument = new CommandArgument
            {
                Name = name,
                Description = description,
                MultipleValues = multipleValues
            };
            AddArgument(argument);
            configuration(argument);
            return argument;
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Adds a command line argument with values that should be parsable into <typeparamref name="T" />.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="configuration"></param>
        /// <param name="multipleValues"></param>
        /// <typeparam name="T">The type of the values on the option</typeparam>
        /// <returns></returns>
        public CommandArgument<T> Argument<T>(string name, string description, Action<CommandArgument<T>> configuration, bool multipleValues = false)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            var parser = ValueParsers.GetParser<T>();

            if (parser == null)
            {
                throw new InvalidOperationException(Strings.CannotDetermineParserType(typeof(T)));
            }

            var argument = new CommandArgument<T>(parser)
            {
                Name = name,
                Description = description,
                MultipleValues = multipleValues
            };
            AddArgument(argument);
            configuration(argument);
            return argument;
        }

        /// <summary>
        /// Add an argument to the definition of this command.
        /// </summary>
        /// <param name="argument"></param>
        public void AddArgument(CommandArgument argument)
        {
            var lastArg = Arguments.LastOrDefault();
            if (lastArg != null && lastArg.MultipleValues)
            {
                throw new InvalidOperationException(Strings.OnlyLastArgumentCanAllowMultipleValues(lastArg.Name));
            }
            _arguments.Add(argument);
        }

        /// <summary>
        /// Defines a callback for when this command is invoked.
        /// </summary>
        /// <param name="invoke"></param>
        public void OnExecute(Func<int> invoke)
        {
            _handler = _ => Task.FromResult(invoke());
        }

        /// <summary>
        /// Defines an asynchronous callback.
        /// </summary>
        /// <param name="invoke"></param>
        public void OnExecuteAsync(Func<CancellationToken, Task<int>> invoke)
        {
            _handler = invoke;
        }

        private void Reset()
        {
            foreach (var arg in Arguments)
            {
                arg.Reset();
            }

            foreach (var option in Options)
            {
                option.Reset();
            }

            foreach (var cmd in Commands)
            {
                cmd.Reset();
            }

            IsShowingInformation = default;
            _remainingArguments.Clear();
        }

        /// <summary>
        /// Adds an action to be invoked when all command line arguments have been parsed and validated.
        /// </summary>
        /// <param name="action">The action to be invoked</param>
        public void OnParsingComplete(Action<ParseResult> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _onParsingComplete ??= new List<Action<ParseResult>>();
            _onParsingComplete.Add(action);
        }

        /// <summary>
        /// Parses an array of strings, matching them against <see cref="Options"/>, <see cref="Arguments"/>, and <see cref="Commands"/>.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>The result of parsing.</returns>
        public ParseResult Parse(params string[] args)
        {
            Reset();

            args ??= Util.EmptyArray<string>();

            var processor = new CommandLineProcessor(this, args);
            var result = processor.Process();
            result.SelectedCommand.HandleParseResult(result);
            return result;
        }

        /// <summary>
        /// When an invalid argument is given, make suggestions in the error message
        /// about similar, valid commands or options.
        /// <para>
        /// $ git pshu
        /// Specify --help for a list of available options and commands
        /// Unrecognized command or argument 'pshu'
        ///
        /// Did you mean this?
        ///     push
        /// </para>
        /// </summary>
        public bool MakeSuggestionsInErrorMessage { get; set; } = true;

        /// <summary>
        /// Handle the result of parsing command line arguments.
        /// </summary>
        /// <param name="parseResult">The parse result.</param>
        protected virtual void HandleParseResult(ParseResult parseResult)
        {
            Parent?.HandleParseResult(parseResult);

            try
            {
                foreach (var option in Options)
                {
                    if (option is IInternalCommandParamOfT o)
                    {
                        o.Parse(ValueParsers.ParseCulture);
                    }
                }

                foreach (var argument in Arguments)
                {
                    if (argument is IInternalCommandParamOfT a)
                    {
                        a.Parse(ValueParsers.ParseCulture);
                    }
                }

                if (_onParsingComplete != null)
                {
                    foreach (var action in _onParsingComplete)
                    {
                        action?.Invoke(parseResult);
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new CommandParsingException(this, ex.Message, ex);
            }
        }

        /// <summary>
        /// Parses an array of strings using <see cref="Parse(string[])"/>.
        /// <para>
        /// If <see cref="OptionHelp"/> was matched, the generated help text is displayed in command line output.
        /// </para>
        /// <para>
        /// If <see cref="OptionVersion"/> was matched, the generated version info is displayed in command line output.
        /// </para>
        /// <para>
        /// If there were any validation errors produced from <see cref="GetValidationResult"/>, <see cref="ValidationErrorHandler"/> is invoked.
        /// </para>
        /// <para>
        /// If the parse result matches this command, the function passed to <see cref="OnExecute"/> or <see cref="OnExecuteAsync"/> will be invoked.
        /// </para>
        /// </summary>
        /// <param name="args"></param>
        /// <returns>The return code from the function passed to <see cref="OnExecute"/> or <see cref="OnExecuteAsync"/>.</returns>
        public int Execute(params string[] args)
        {
            return ExecuteAsync(args).GetAwaiter().GetResult();
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Parses an array of strings using <see cref="Parse(string[])"/>.
        /// <para>
        /// If <see cref="OptionHelp"/> was matched, the generated help text is displayed in command line output.
        /// </para>
        /// <para>
        /// If <see cref="OptionVersion"/> was matched, the generated version info is displayed in command line output.
        /// </para>
        /// <para>
        /// If there were any validation errors produced from <see cref="GetValidationResult"/>, <see cref="ValidationErrorHandler"/> is invoked.
        /// </para>
        /// <para>
        /// If the parse result matches this command, the function passed to <see cref="OnExecute"/> or <see cref="OnExecuteAsync"/> will be invoked.
        /// </para>
        /// </summary>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The return code from the function passed to <see cref="OnExecute"/> or <see cref="OnExecuteAsync"/>.</returns>
        public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            var parseResult = Parse(args);
            var command = parseResult.SelectedCommand;

            if (command.IsShowingInformation)
            {
                return HelpExitCode;
            }

            var validationResult = command.GetValidationResult();
            if (validationResult != ValidationResult.Success)
            {
                return command.ValidationErrorHandler(validationResult);
            }

            var handlerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            void cancelHandler(object o, ConsoleCancelEventArgs e)
            {
                handlerCancellationTokenSource.Cancel();
            }

            try
            {
                _context.Console.CancelKeyPress += cancelHandler;

                return await command._handler(handlerCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return s_exitCodeOperationCanceled;
            }
            finally
            {
                _context.Console.CancelKeyPress -= cancelHandler;
            }
        }

        /// <summary>
        /// Helper method that adds a help option.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public CommandOption HelpOption(string template)
            => HelpOption(template, false);

        /// <summary>
        /// Helper method that adds a help option.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        public CommandOption HelpOption(string template, bool inherited)
        {
            // Help option is special because we stop parsing once we see it
            // So we store it separately for further use
            OptionHelp = Option(template, Strings.DefaultHelpOptionDescription, CommandOptionType.NoValue, inherited);

            return OptionHelp;
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Helper method that adds a version option from known versions strings.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="shortFormVersion"></param>
        /// <param name="longFormVersion"></param>
        /// <returns></returns>
        public CommandOption VersionOption(string template,
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            string? shortFormVersion,
            string? longFormVersion = null)
        {
            return longFormVersion == null
                ? VersionOption(template, () => shortFormVersion)
                : VersionOption(template, () => shortFormVersion, () => longFormVersion);
        }

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// Helper method that adds a version option.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="shortFormVersionGetter"></param>
        /// <param name="longFormVersionGetter"></param>
        /// <returns></returns>
        public CommandOption VersionOption(string template,
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            Func<string?>? shortFormVersionGetter,
            Func<string?>? longFormVersionGetter = null)
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
        public virtual void ShowHint()
        {
            if (OptionHelp != null)
            {
                var flag = !string.IsNullOrEmpty(OptionHelp.LongName)
                    ? "--" + OptionHelp.LongName
                    : !string.IsNullOrEmpty(OptionHelp.ShortName)
                        ? "-" + OptionHelp.ShortName
                        : "-" + OptionHelp.SymbolName;

                Out.WriteLine($"Specify {flag} for a list of available options and commands.");
            }
        }

        /// <summary>
        /// Show full help.
        /// </summary>
        public void ShowHelp() => ShowHelp(usePager: UsePagerForHelpText);

        /// <summary>
        /// Show full help.
        /// </summary>
        /// <param name="usePager">Use a console pager to display help text, if possible.</param>
        public void ShowHelp(bool usePager)
        {
            var cmd = this;
            while (cmd != null)
            {
                cmd.IsShowingInformation = true;
                cmd = cmd.Parent;
            }

            if (usePager && ReferenceEquals(Out, _context.Console.Out))
            {
                using var pager = new Pager(_context.Console);
                _helpTextGenerator.Generate(this, pager.Writer);
            }
            else
            {
                _helpTextGenerator.Generate(this, Out);
            }
        }

        /// <summary>
        /// Produces help text describing command usage.
        /// </summary>
        /// <returns>The help text.</returns>
        public virtual string GetHelpText()
        {
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            _helpTextGenerator.Generate(this, writer);
            return sb.ToString();
        }

        /// <summary>
        /// Displays version information that includes <see cref="FullName"/> and <see cref="LongVersionGetter"/>.
        /// </summary>
        public void ShowVersion()
        {
            var cmd = this;
            while (cmd != null)
            {
                cmd.IsShowingInformation = true;
                cmd = cmd.Parent;
            }

            Out.Write(GetVersionText());
        }

        /// <summary>
        /// Produces text describing version of the command.
        /// </summary>
        /// <returns>The version text.</returns>
        public virtual string GetVersionText()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(FullName))
            {
                sb.AppendLine(FullName);
            }

            if (LongVersionGetter != null)
            {
                sb.AppendLine(LongVersionGetter());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets <see cref="FullName"/> and <see cref="ShortVersionGetter"/>.
        /// </summary>
        /// <returns></returns>
        public virtual string GetFullNameAndVersion()
        {
            var items = new List<string?>
            {
                FullName,
                ShortVersionGetter?.Invoke()
            };

            return string.Join(" ", items.Where(i => !string.IsNullOrEmpty(i)));
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

        internal bool MatchesName(string name)
        {
            return string.Equals(name, Name, StringComparison.OrdinalIgnoreCase) || _names.Contains(name);
        }

        private sealed class Builder : IConventionBuilder
        {
            private readonly CommandLineApplication _app;

            public Builder(CommandLineApplication app)
            {
                _app = app;
            }

            IConventionBuilder IConventionBuilder.AddConvention(IConvention convention)
            {
                convention.Apply(_app._conventionContext);

                foreach (var command in _app.Commands)
                {
                    command.Conventions.AddConvention(convention);
                }

                _app._conventions.Add(convention);
                return _app.Conventions;
            }
        }

        private IConventionBuilder? _builder;

        /// <summary>
        /// Gets a builder that can be used to apply conventions to
        /// </summary>
        public IConventionBuilder Conventions => _builder ??= new Builder(this);

        private protected virtual ConventionContext CreateConventionContext() => new(this, null);

        private bool _settingContext;
        internal void SetContext(CommandLineContext context)
        {
            if (_settingContext)
            {
                // prevent stack overflow in the event someone has looping command line apps
                return;
            }

            _settingContext = true;
            _context = context;
            Out = context.Console.Out;
            Error = context.Console.Error;

            foreach (var cmd in Commands)
            {
                cmd.SetContext(context);
            }

            _settingContext = false;
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            foreach (var command in Commands)
            {
                if (command is IDisposable dc)
                {
                    dc.Dispose();
                }
            }
        }

        internal IServiceProvider? AdditionalServices { get; set; }

        object IServiceProvider.GetService(Type serviceType) => _services.Value.GetService(serviceType);

        private sealed class ServiceProvider : IServiceProvider
        {
            private readonly CommandLineApplication _parent;

            public ServiceProvider(CommandLineApplication parent)
            {
                _parent = parent;
            }

            public object? GetService(Type serviceType)
            {
                if (typeof(object) == serviceType)
                {
                    // this type is too generic. Skip this one.
                    return null;
                }

                if (serviceType == typeof(CommandLineApplication))
                {
                    return _parent;
                }

                if (serviceType == _parent.GetType())
                {
                    return _parent;
                }

                // prefer this type before AdditionalServices because it is common for service containers to automatically
                // create IEnumerable<T> to allow registration of multiple services
                if (serviceType == typeof(IEnumerable<CommandOption>))
                {
                    return _parent.GetOptions();
                }

                if (serviceType == typeof(IEnumerable<CommandArgument>))
                {
                    return _parent.Arguments;
                }

                if (serviceType == typeof(CommandLineContext))
                {
                    return _parent._context;
                }

                if (serviceType == typeof(IServiceProvider))
                {
                    return this;
                }

                if (_parent.Parent is IModelAccessor accessor && serviceType == accessor.GetModelType())
                {
                    return accessor.GetModel();
                }

                var retVal = _parent.AdditionalServices?.GetService(serviceType);
                if (retVal != null)
                {
                    return retVal;
                }

                // Resolve this after AdditionalServices to support overriding IConsole from a custom service container
                // may be overridden
                if (serviceType == typeof(IConsole))
                {
                    return _parent._context.Console;
                }

                return null;
            }
        }
    }
}
