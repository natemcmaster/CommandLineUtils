// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Adds a help option of --help if no other help option is specified.
    /// </summary>
    public class DefaultHelpOptionConvention : IConvention
    {
        /// <summary>
        /// The default help template.
        /// </summary>
        public const string DefaultHelpTemplate = Strings.DefaultHelpTemplate;
        private readonly string _template;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultHelpOptionConvention"/>.
        /// </summary>
        /// <param name="template"></param>
        public DefaultHelpOptionConvention(string template)
        {
            _template = template;
        }

        /// <inheritdoc />
        public void Apply(ConventionContext context)
        {
            if (context.Application.OptionHelp != null)
            {
                return;
            }

            if (context.ModelType != null
                && (context.ModelType.GetCustomAttribute<SuppressDefaultHelpOptionAttribute>() != null
                || context.ModelType.Assembly.GetCustomAttribute<SuppressDefaultHelpOptionAttribute>() != null))
            {
                return;
            }

            var help = new CommandOption(_template, CommandOptionType.NoValue)
            {
                Description = Strings.DefaultHelpOptionDescription,

                // the convention will run on each subcommand automatically.
                // it is better to run the command on each to check for overlap
                // or already set options to avoid conflict
                Inherited = false,
            };

            foreach (var opt in context.Application.GetOptions())
            {
                if (string.Equals(help.LongName, opt.LongName))
                {
                    help.LongName = null;
                }
                if (string.Equals(help.ShortName, opt.ShortName))
                {
                    help.ShortName = null;
                }

                if (string.Equals(help.SymbolName, opt.SymbolName))
                {
                    help.SymbolName = null;
                }
            }

            if (help.LongName != null || help.ShortName != null || help.SymbolName != null)
            {
                context.Application.OptionHelp = help;
                context.Application.AddOption(help);
            }
        }
    }
}
