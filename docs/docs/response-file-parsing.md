---
uid: response-file-parsing
---
# @-files (Response File Parsing)

**CommandLineUtils** support parsing of response files. The command-line parser treats arguments beginning with '@' as a file path to a response file.

    myapp.exe @args.txt

A response file contains additional arguments that will be treated as if they were passed in on the command line.

* Response files can have comments that begin with the # symbol.
* You cannot use the backslash character (`\`) to concatenate lines.

By default, response file parsing is disabled for your application and all sub-commands. You can enable response file parsing using either the Builder API or Attributes.

# [Using Attributes](#tab/using-attributes)

When using Attributes, you can enable response file parsing by setting the @McMaster.Extensions.CommandLineUtils.CommandAttribute.ResponseFileHandling property of the @McMaster.Extensions.CommandLineUtils.CommandAttribute.

[!code-csharp[](../samples/response-file-parsing/attributes/Program.cs?range=6-34&highlight=1)]

# [Using Builder API](#tab/using-builder-api)

When using the Builder API, you can enable response file parsing by setting the @McMaster.Extensions.CommandLineUtils.CommandLineApplication.ResponseFileHandling property of the @McMaster.Extensions.CommandLineUtils.CommandLineApplication.

[!code-csharp[](../samples/response-file-parsing/builder-api/Program.cs?range=6-38&highlight=9)]

------


In the example above, the `ResponseFileHandling` property has been set to `ResponseFileHandling.ParseArgsAsLineSeparated` meaning that each argument or option will be on its own line.

Let's assume that you want to run the application with the following command:

```text
done "Completed the Boston marathon" --tag major --tag fitness
```

You can achieve the same result by creating a file called `input.txt` with the following contents:

```text
Completed the Boston marathon
--tag
major
--tag
fitness
```

And then passing that file on the command-line instead:

```text
done @input.txt
```

## Using in combination with sub-commands

You can specify sub-commands in the response file, and **CommandLineUtils** will execute the correct sub-command. Given the previous code sample, when passing the following response file

```text
list
--tag
major
--tag
fitness
```

The `list` command will be executed, and the values of `major` and `fitness` will be passed for the `Tags` option.

## Combining response files with arguments

You can pass a combination of command-line arguments and response files. For example, you can specify the following response file:

```text
--tag
major
--tag
fitness
```

And then pass that in combination with other command-line arguments, e.g.:

```text
done "Completed the Boston marathon" @input.txt
```

This would be the equivalent of executing

```text
done "Completed the Boston marathon" --tag major --tag fitness
```

You can also use it in combination with sub-commands:

```text
done list @input.txt
```

This will execute the `list` command and pass the values of `major` and `fitness` for the `Tags` option.

When using sub-commands, you need to take care to explicitly set the `ResponseFileHandling` property for the sub-commands as well.

# [Using Attributes](#tab/using-attributes)

[!code-csharp[](../samples/response-file-parsing/attributes/Program.cs?range=6-34&highlight=19)]

# [Using Builder API](#tab/using-builder-api)

[!code-csharp[](../samples/response-file-parsing/builder-api/Program.cs?range=6-40&highlight=18)]

------

## Space separated arguments

You can set the `ResponseFileHandling` property to @McMaster.Extensions.CommandLineUtils.ResponseFileHandling.ParseArgsAsSpaceSeparated. In this case, each argument in the response file needs to be separated by a space, instead of a new line, e.g.

```text
"Completed the Boston marathon" --tag major --tag fitness
```
