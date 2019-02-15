---
uid: arguments
---

# Arguments

When a command executes, the raw `string[] args` value can be separated into two different categories: options and arguments.

Arguments are positional and values are specified based by order.

Options are named and must be specified using a name. Options are covered in [this document.](xref:options)

Arguments are represented by the @McMaster.Extensions.CommandLineUtils.CommandArgument type.
They have one defining characteristic.

* Position - the order in which arguments appear on command line (after options have been parsed)

### [Using Attributes](#tab/using-attributes)

@McMaster.Extensions.CommandLineUtils.ArgumentAttribute can be used on properties to define arguments.
The argument order must be specified explicitly.

```c#
public class Program
{
    [Argument(0)]
    [Required]
    public string FirstName { get; }

    [Argument(1)]
    public string LastName { get; }  // this one is optional because it doesn't have `[Required]`

    private void OnExecute()
    {
        Console.WriteLine($"Hello {FirstName} {LastName}");
    }

    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
}
```

### [Using Builder API](#tab/using-builder-api)

```c#
public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        var firstNameArg = app.Argument("first name", "the first name of the person")
            .IsRequired();
        var lastNameArg = app.Argument("last name", "the last name of the person");

        app.OnExecute(() =>
        {
            Console.WriteLine($"Hello {firstNameArg.Value} {lastNameArg.Value}");
        });

        return app.Execute(args);
    }
}
```

***

## Variable numbers of arguments

A common scenario is to allow a command line tool to take in a variable number of arguments.
These arguments are collected into a `string[]` array after all other arguments and options are parsed.

```
cat -b 123 file1.txt file2.txt file3.txt
```

### [Using Attributes](#tab/using-attributes)

By default, attribute binding will assume multiple values can be set for properties with the `[Argument]`
attribute and settable to `string[]` or `IEnumerable<string>`.

```c#
public class Program
{
    [Option("-b")]
    public int BlankLines { get; }

    [Argument(0)]
    public string[] Files { get; } // = { "file1.txt", "file2.txt", "file3.txt" }

    private void OnExecute()
    {
        if (Files != null)
        {
            foreach (var file in Files)
            {
                // do something
            }
        }
    }

    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
}
```

### [Using Builder API](#tab/using-builder-api)

To enable this, @McMaster.Extensions.CommandLineUtils.CommandArgument.MultipleValues must be set to true,
and the argument must be the last one specified.

```c#
public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();
        var blankLines = app.Option<int>("-b <LINES>", "Blank lines", CommandOptionType.SingleValue);
        var files = app.Argument("Files", "Files to count", multipleValues: true);
        app.OnExecute(() =>
        {
            foreach (var file in files.Values)
            {
                // do something
            }
        });
        return app.Execute(args);
    }
}
```

***

## Pass-thru arguments

Another common scenario is to create a command line tool which wraps another tool. These kinds of command lines
need to collect arguments which are passed to the to the tool they wrap. For example, the Unix command `time`
or the Windows command `cmd` take some arguments, and pass the rest on to the command they invoke.

> [!NOTE]
> Example:
>
> ```
> time -l ls -a -l ./
> ```
>
> In this example, `-l` is an option on `time`. This starts a timer which then invokes `ls` with additional arguments.
> `-l` is also an option on `ls`.

Normally, unrecognized arguments is an error. You must set ThrowOnUnexpectedArgument to `false` to allow the parser
to allow unrecognized arguments and options.

### The double-dash convention `--`

It is common for apps which pass-thru arguments to allow the caller to use `--` to distinguish between the
options on the parent command and all remaining arguments.

```
bash -c -- ls -a -l
```

In this example, the presence of `--` forces bash to stop parsing and treat everything after `--` as an argument
to be passed to the inner command.

The double dash command is enabled by setting @McMaster.Extensions.CommandLineUtils.CommandLineApplication.AllowArgumentSeparator.

### [Using Attributes](#tab/using-attributes)

By default, attribute binding will set a `string[]` or `IList<string>` property named `RemainingArguments` or `RemainingArgs`
to include all values. See @McMaster.Extensions.CommandLineUtils.Conventions.RemainingArgsPropertyConvention for more details.

[!code-csharp[Program](../samples/passthru-args/attributes/Program.cs)]

### [Using Builder API](#tab/using-builder-api)

When `throwOnUnexpctedArg` is set to false,

[!code-csharp[Program](../samples/passthru-args/builder-api/Program.cs)]
