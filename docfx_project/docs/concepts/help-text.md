# Help Text

CommandLineUtils provides API to automatically generate help text for a command line application.
By default, the help text will only be shown if you indicate that a help option should exit.

## Adding a help option

Using the **builder API**, call [`.HelpOption()`](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplicationExtensions.HelpOption(McMaster.Extensions.CommandLineUtils.CommandLineApplication)).
```c#
var app = new CommandLineApplication();
app.HelpOption();
```

This adds three flags to the command line app that will show help, `-?`, `-h`, and `--help`.
You can change these flags by calling [`.HelpOption(string template)`](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplication.HelpOption(System.String)) with a template string.

```c#
app.HelpOption("-m|--my-help");
```

When using the **attribute API**, add an instance of [HelpOptionAttribute](xref:McMaster.Extensions.CommandLineUtils.HelpOptionAttribute) to your model type.

```c#
[HelpOption]
public class Program
```

## Add a help option to all subcommands

If you have subcommands on your application, you can make a help option available on all subcommands without
needing to explicitly add a HelpOption to each subcommand type or object. To do this, set `inherited` to true
when adding the help option.

```c#
app.HelpOption(inherited: true);
```

```c#
[HelpOption(Inherited = true)]
public class Program
```

## Extending help text

By default, the help text only includes information about arguments, options, and commands.
If you would like to provide additional information, you can use the
[ExtendedHelpText](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplication.ExtendedHelpText)
property to add additional information to help output.

```c#
var app = new CommandLineApplication();
app.ExtendedHelpText = @"
Remarks:
  This command should only be used on Tuesdays.
";
```

```c#
[Command(
    ExtendedHelpText = @"
Remarks:
  This command should only be used on Tuesdays.
"
)]
public class Program

```

## Custom help text

Help text generation can be completely customized by implementing the
[IHelpTextGenerator](xref:McMaster.Extensions.CommandLineUtils.HelpText.IHelpTextGenerator)
interface.

```c#
class MyHelpTextGenerator : IHelpTextGenerator
{
    public void void Generate(CommandLineApplication application, TextWriter output)
    {
        output.WriteLine(@"To use this command, throw salt over your shoulder and spit twice.");
    }
}

class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();
        app.HelpTextGenerator = new MyHelpTextGenerator();
        return app.Execute(args);
    }
}
```
