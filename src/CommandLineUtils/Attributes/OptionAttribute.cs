using System;

namespace McMaster.Extensions.CommandLineUtils
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        public OptionAttribute()
        {
        }

        public OptionAttribute(string template)
        {
            Template = template;
        }

        public OptionAttribute(CommandOptionType optionType)
        {
            OptionType = optionType;
        }

        public OptionAttribute(string template, CommandOptionType optionType)
        {
            Template = template;
            OptionType = optionType;
        }

        public string Template { get; set; }
        public CommandOptionType? OptionType { get; set; }
        public string Description { get; set; }
        public bool ShowInHelpText { get; set; } = true;
        public bool Inherited { get; set; }
    }
}
