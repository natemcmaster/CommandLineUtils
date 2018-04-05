using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;

/// <summary>
/// A custom class for demonstration / learning with debug purposes.
/// It acts as a model and has sub-commands specified both ways (through API and through attributes).
/// </summary>
[Subcommand("nestedCmd", typeof(NestedAttrbutedCommand))]
public class CustomDemo
{

    public CustomDemo()
    {
    }

    public static int Main(string[] args)
    {
        //var app = new CommandLineApplication();
        var app = new CommandLineApplication<CustomDemo>();
        app.HelpOption(inherited: true);
        var debugOption = app.Option("--debug", "Application outputs even since debug log events to the console", CommandOptionType.NoValue);
        var verboseOption = app.Option("-verb|--verbose", "Application outputs even since verbose log events to the console", CommandOptionType.NoValue);

        app.Conventions
            .UseDefaultConventions();
            //.AddConvention(new CustomConvention);

        // cmd1 command
        app.Command("cmd1", cmd =>
        {
            cmd.FullName = "App - cmd1";
            cmd.Description = "cmd1 description";
            cmd.OnExecute(() =>
            {
                Console.WriteLine(cmd.FullName);
                return 0;
            });
        });

        // cmd2 command
        app.Command("cmd2", cmd =>
        {
            cmd.FullName = "App - cmd2";
            cmd.Description = "cmd2 description";
            cmd.OnExecute(() =>
            {
                Console.WriteLine(cmd.FullName);
                return 0;
            });
        });

        // the main command
        app.OnExecute(async () =>
        {
            await Task.Delay(1000);
            return 0;
        });

        app.OnParsingComplete((parseResult) =>
        {
            var isDebug = debugOption.HasValue();
            var isVerbose = verboseOption.HasValue();
            //configure logger with given level output to console
            // LogerUtils.Initialize(..., LogerUtils.GetConsoleLogLevelAmendment(isDebug, isVerbose), ...);
        });

        return app.Execute(args);
    }


    [Command(Name = "nestedCmd", Description = "The nested command.", ExtendedHelpText = "extended help text")]
    class NestedAttrbutedCommand
    {
        private void OnExecute(IConsole console)
        {
            console.WriteLine("nestedCmd execution...");
        }
    }
}

class CustomConvention : IConvention
{
    public static  int ApplyCount = 0;

    public CustomConvention()
    {
    }

    public void Apply(ConventionContext context)
    {
        ApplyCount++;
    }
}
