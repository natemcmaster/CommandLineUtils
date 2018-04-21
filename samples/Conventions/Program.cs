// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

public class Program
{
    static void Main(string[] args)
    {
        // This sample shows you how to use dependency injection along with the constructor injection convention
        DependencyInjectionProgram.Main(args);

        // This sample shows you how to write your own conventions to bind a type's methods to a subcommand
        MethodsAsSubcommandsProgram.Main(args);

        // This sample shows you how to write conventions as an attribute.
        AttributeConventionProgram.Main(args);
    }
}
