using System;
using McMaster.Extensions.CommandLineUtils;

namespace ResponseFileParsing
{
    class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "done",
                Description = "Keep track on things you've done",
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated
            };

            app.HelpOption(inherited: true);
            var argumentDescription = app.Argument("Description", "The description of what you've done");
            var optionTags = app.Option("-t|--tag <TAGS>", "A tag for the item", CommandOptionType.MultipleValue);

            app.Command("list", listCommand =>
            {
                listCommand.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;

                var optionListTags = listCommand.Option("-t|--tag <TAGS>", "Only list items with the corresponding tag(s)", CommandOptionType.MultipleValue);

                listCommand.OnExecute(() =>
                {
                    //...
                });
            });

            app.OnExecute(() =>
            {
                //...
            });

            return app.Execute(args);
        }
    }
}
