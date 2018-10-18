---
uid: options
---

# Options

When a command executes, the raw `string[] args` value can be separated into two different categories: options and arguments.

Options are named and must be specified using a name. By default, options are optional and order does not matter,
but they can be made mandatory.

Arguments are positional and values are specified based by order. Arguments are covered in [this document.](xref:arguments)

```
mycommand.exe abc --verbose --path:logs/ --message=Hello xyz
```

This sample breaks down in the following way:

String | Interpretation
-------|----------
`mycommand.exe` | the name of the command (handled by the operating system. In .NET, this value isn't part of `string[] args`.
`abc` | Argument (position 0)
`--path:logs/` | Option. Name = "path", value = "logs/"
`--verbose` | Option. Name = "verbose", value = null
`--message=Hello` | Option. Name = "message", value = "Hello"
`xyz` | Argument (position 1)


Options are represented by the @McMaster.Extensions.CommandLineUtils.CommandOption type.
They have two defining characteristics.

 * Names - options can be identified by multiple names, such as a long name (e.g. "message") or a short name, which is usually a single character.
   An option must have at least one name.
    * Short names are used with a single dash `-v`
    * Long names are used with two dashes `--verbose`
 * Type
    * No value - options do not have a value.
      They are either considered "specified" or "absent".
      These are also sometimes called flags or switches.
      `--verbose`
    * Single value - an option which must have a single value specified.
      The value can be specified with ' ', ':' or '=' to separate the value from the name.
      `--name value`, `--name=value`, `--name:value`
    * Multiple values - an option can be specified multiple times with multiple values.
      `--name one --name two`
    * Single or no value - a special case of "no value" options where an value may or may not be specified.
      They can be specified as `--name` (no value) or `--name:value` or `--name=value`.
      Unlike "single value", these cannot be specified as `--name value` because the space causes ambiguous usage.

### [Using Attributes](#tab/using-attributes)

@McMaster.Extensions.CommandLineUtils.OptionAttribute can be used on properties to define options.
When specified, name and type are inferred, but they can be listed explicitly.
The inferred short name (with "-" prefix) is the first letter of property name in lowercase.
The inferred long name (with "--" prefix) is the property name in [kebab case](https://en.wikipedia.org/wiki/Letter_case) (e.g. "--log-level" for LogLevel).
Note that option names are case sensitive, and using different case is an error, but error message suggests expected lowercase spelling.


```c#
public class Program
{
    [Option]
    public bool Verbose { get; set; }
    // Inferred type = NoValue
    // Inferred names = "-v", "--verbose"

    [Option]
    public string Color { get; set; }
    // Inferred type = SingleValue
    // Inferred names = "-c", "--color"

    [Option]
    public (bool hasValue, string value) LogLevel { get; set; }
    // Inferred type = SingleOrNoValue
    // Inferred names = "-l", "--log-level"

    [Option("-N")]
    public int[] Names { get; set; }
    // Inferred type = MultipleValues
    // Defined names = "-N"

    [Option("-A", CommandOptionType.SingleValue)]
    public int[] Area { get; set; }
    // Defing type = SingleValue
    // Defined names = "-A"

    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
}
```

### [Using Builder API](#tab/using-builder-api)

When using the builder API, name and type must be specified explicitly.

```c#
public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        var verbose = app.Option("-v|--verbose", "Show verbose output", CommandOptionType.NoValue);
        var color = app.Option("-c|--color <COLOR>", "A color", CommandOptionType.SingleValue);
        var logLevel = app.Option("-l|--log-level[:<LEVEL>]", "The log level", CommandOptionType.SingleOrNoValue);
        var names = app.Option("-n|--names <NAME>", "Names", CommandOptionType.MultipleValue);

        return app.Execute(args);
    }
}
```

***

## Flag counting

A common scenario for options is to allow specifying a value-less option 
multiple times without value. The library supports counting flags by using `bool[]`
or by checking for the number of values in @McMaster.Extensions.CommandLineUtils.CommandOption.Values.

### [Using Attributes](#tab/using-attributes)

Requires **2.3** and newer.

```c#
//
// Expected
//

public class Program
{
    [Option]
    public bool[] Verbose { get; set; }
    
    public void OnExecute()
    {
       Console.WriteLine("Verbose count = " + Verbose.Length);
    }
  
    public static int Main(string[] args) 
        // result: "Verbose count = 3"
        => CommandLineApplication.Execute<Program>("-v", "-v", "-v");
}
```

### [Using Builder API](#tab/using-builder-api)

When using the builder API, name and type must be specified explicitly.

```c#
public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        var verbose = app.Option("-v|--verbose", "Show verbose output", CommandOptionType.NoValue);
   
        app.OnExecute(()
            => Console.WriteLine("Verbose count = " + verbose.Values.Count); );
   
        // result: "Verbose count = 3"
        return app.Execute("-v", "-v", "-v");
    }
}
```

***

## Examples

The follow examples show how different inputs will be handled.


```c#
[Option("-v|--Verbose")]
public bool Verbose { get; }
```

Inputs |  Value of `Verbose`
-------|---
(not specified) | `false`
`--verbose` | `true`

When you override the default option type to `CommandOptionType.SingleValue`, you get different behavior.

```c#
[Option("-v|--Verbose", CommandOptionType.SingleValue)]
public bool Verbose { get; }
```

Inputs | Value of `Verbose`
-------|---
 (not specified) | `false`
`--verbose` | Invalid. Value expected after `--verbose`.
`--verbose true` | `true`
`--verbose false` | `false`
`--verbose banana` | Error. Cannot parse `"banana"` into `System.Boolean`.

If "Verbose" were a string type:

```c#
[Option("-v|--Verbose", CommandOptionType.SingleValue)]
public string Verbose { get; }
```

Inputs | Value of `Verbose`
-------|---
 (not specified) | `null`
`--verbose` | Invalid. Value expected after `--verbose`.
`--verbose true` | `"true"`
`--verbose false` | `"false"`
`--verbose banana` | `"banana"`

If "Verbose" accepted multiple values:

```c#
[Option("-v|--Verbose", CommandOptionType.MultipleValue)]
public string[] Verbose { get; }
```

Inputs | Value of `Verbose`
-------|---
 (not specified) | `null`
`--verbose` | Invalid. Value expected after `--verbose`.
`--verbose banana` | `{ "banana" }`
`--verbose banana --verbose strawberry` | `{ "banana", "strawberry" }`

If "Verbose" accepted single or value:

```c#
[Option("-v|--Verbose", CommandOptionType.SingleOrNoValue)]
public (bool hasValue, string value) Verbose { get; }
```

Inputs | Value of `Verbose`
-------|---
 (not specified) | `(false, null)`
`--verbose` | `(true, null)`
`--verbose:banana` | `(true, "banana")`
`--verbose=banana` | `(true, "banana")`
`--verbose banana` | Invalid. SingleOrNoValue options cannot use a space delimiter between option name and value. 
