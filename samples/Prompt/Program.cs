using System;
using McMaster.Extensions.CommandLineUtils;

class Program
{
    static void Main(string[] args)
    {
        var proceed = Prompt.GetYesNo("Do you want to proceed with this demo?",
            defaultAnswer: true,
            promptColor: ConsoleColor.Black,
            promptBgColor: ConsoleColor.White);

        if (!proceed) return;

        var name = Prompt.GetString("What is your name?",
            promptColor: ConsoleColor.White,
            promptBgColor: ConsoleColor.DarkGreen);

        Console.WriteLine($"Hello, there { name ?? "anonymous console user"}.");

        var age = Prompt.GetInt("How old are you?",
            promptColor: ConsoleColor.White,
            promptBgColor: ConsoleColor.DarkRed);

        var password = Prompt.GetPassword("What is your password?",
            promptColor: ConsoleColor.White,
            promptBgColor: ConsoleColor.DarkBlue);

        Console.Write($"Your password contains {password.Length} characters. ");
        switch (password.Length)
        {
            case int _ when (password.Length < 2):
                Console.WriteLine("Your password is so short you might as well not have one.");
                break;
            case int _ when (password.Length < 4):
                Console.WriteLine("Your password is too short. You should pick a better one");
                break;
            case int _ when (password.Length < 10):
                Console.WriteLine("Your password is too okay, I guess.");
                break;
            default:
                Console.WriteLine("Your password is probably adequate.");
                break;
        }
    }
}
