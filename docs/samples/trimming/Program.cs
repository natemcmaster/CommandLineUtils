using System;
using McMaster.Extensions.CommandLineUtils;

// Minimal command class to exercise reflection-based constructor injection and OnExecute method.
public class MyCommand
{
    // Public constructor to be preserved by trimming
    public MyCommand()
    {
    }

    public int OnExecute()
    {
        Console.WriteLine("MyCommand executed");
        return 0;
    }
}

class Program
{
    static int Main(string[] args)
    {
        // Generic Execute should be annotated in the library to preserve public methods/constructors
        return CommandLineApplication.Execute<MyCommand>(args);
    }
}
