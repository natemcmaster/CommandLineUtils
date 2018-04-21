// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        // There are 4 sample project has a few simple entry programs.

        // The builder API allows you to build an object of type CommandLineApplication
        // and add new commands and settings to it.
        BuilderApi.Main(args);

        // The attribute pattern is more declarative. You can define arguments and options
        // as properties on a type and use attributes to mark how the properties should be treated.
        Attributes.Main(args);

        // This async example shows how to use an async method in your command line application.
        AsyncWithBuilderApi.Main(args);

        // This async example shows how to use an async method with attributes.
        await AsyncWithAttributes.Main(args);
    }
}
