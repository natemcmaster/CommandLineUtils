---
uid: help-text
---

# Help Text

CommandLineUtils provides API to automatically generate help text for a command line application.

## Configure the help option

### Attribute API
By default, three options will exist on the command line app that will show help: `-?`, `-h`, and `--help`.
When users specify one of these, generated help text will display on the command line.

```
> MyApp.exe --help

Usage: MyApp.exe [options]

Options:
  -?|-h|--help   Show help output
```

To customize the flags that can show help text, add an instance of [HelpOptionAttribute](xref:McMaster.Extensions.CommandLineUtils.HelpOptionAttribute) to your model type.

```csharp
[HelpOption("--my-custom-help-option")]
public class Program
```

```
> MyApp.exe --help
Unrecognized option '--help'

> MyApp.exe --my-custom-help-option

Usage: MyApp.exe [options]

Options:
  --my-custom-help-option   Show help output
```

#### Disabling the default

To disable the help option by default, add `[SuppressDefaultHelpOption]` to your program type or the containing assembly.

```csharp
// suppress the help option on all types define in this project
[assembly: SuppressDefaultHelpOption]

// disable help option on a specific command
[SuppressDefaultHelpOption]
public class MySecretProgram
```

### Builder API
By default, the help text will only be shown if you define a help option on the command.

Using the **builder API**, call [`.HelpOption()`](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplicationExtensions.HelpOption(McMaster.Extensions.CommandLineUtils.CommandLineApplication)).

```csharp
var app = new CommandLineApplication();
app.HelpOption();
```

This adds three flags to the command line app that will show help, `-?`, `-h`, and `--help`.
You can change these flags by calling [`.HelpOption(string template)`](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplication.HelpOption(System.String)) with a template string.

```csharp
app.HelpOption("-m|--my-help");
```

#### Add a help option to all subcommands

If you have subcommands on your application, you can make a help option available on all subcommands without
needing to explicitly add a HelpOption to each subcommand type or object. To do this, set `inherited` to true
when adding the help option.

```csharp
app.HelpOption(inherited: true);
```

## Extending help text

By default, the help text only includes information about arguments, options, and commands.
If you would like to provide additional information, you can use the
[ExtendedHelpText](xref:McMaster.Extensions.CommandLineUtils.CommandLineApplication.ExtendedHelpText)
property to add additional information to help output.

```csharp
var app = new CommandLineApplication();
app.ExtendedHelpText = @"
Remarks:
  This command should only be used on Tuesdays.
";
```

```csharp
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

```csharp
class MyHelpTextGenerator : IHelpTextGenerator
{
    public void Generate(CommandLineApplication application, TextWriter output)
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
