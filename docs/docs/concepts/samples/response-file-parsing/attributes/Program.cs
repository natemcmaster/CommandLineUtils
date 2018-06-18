using System;
using McMaster.Extensions.CommandLineUtils;

namespace ResponseFileParsing
{
    [Command(Name = "done", Description = "Keep track on things you've done", ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated)]
    [Subcommand("list", typeof(ListCommand))]
    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0, "The description of what you've done")]
        public string Description { get; }

        [Option(CommandOptionType.MultipleValue, LongName = "tag", Description = "A tag for the item")]
        public string[] Tags { get; }

        private void OnExecute()
        {
            //...
        }
    }

    [Command(Description = "List all done items", ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated)]
    class ListCommand
    {
        [Option(CommandOptionType.MultipleValue, LongName = "tag", Description = "Only list items with the corresponding tag(s)")]
        public string[] Tags { get; }

        private void OnExecute()
        {
            //...
        }
    }
}
