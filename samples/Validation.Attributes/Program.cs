// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;

class Program
{
    static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    [Required]
    [Option(Description = "Required. The message")]
    private string Message { get; }

    [Required]
    [EmailAddress]
    [Option("--to <EMAIL>", Description = "Required. The recipient.")]
    public string To { get; }

    [Required]
    [EmailAddress]
    [Option("--from <EMAIL>", Description = "Required. The sender.")]
    public string From { get; }

    [Option(Description = "The colors should be red or blue")]
    [RedOrBlue]
    public string Color { get; }

    private void OnExecute()
    {
        Console.WriteLine("From = " + From);
        Console.WriteLine("To = " + To);
        Console.WriteLine("Message = " + Message);
    }
}

class RedOrBlueAttribute : ValidationAttribute
{
    public RedOrBlueAttribute()
        : base("The value for {0} must be 'red' or 'blue'")
    {
    }

    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        if (value == null || (value is string str && str != "red" && str != "blue"))
        {
            return new ValidationResult(FormatErrorMessage(context.DisplayName));
        }

        return ValidationResult.Success;
    }
}
