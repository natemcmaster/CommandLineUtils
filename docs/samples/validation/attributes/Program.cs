// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
// This is required since .NET 8 introduced a new type System.ComponentModel.DataAnnotations.AllowedValuesAttribute
// which conflicts with the attribute in this library (added long before .NET 8.)
using AllowedValues = McMaster.Extensions.CommandLineUtils.AllowedValuesAttribute;

[MaxSizeOptionRequiresAttachmentValidation()]
class AttributeProgram
{
    public static int Main(string[] args) => CommandLineApplication.Execute<AttributeProgram>(args);

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

    [FileExists]
    [Option("--attachment <FILE>")]
    public string[] Attachments { get; }

    [Option]
    [AllowedValues("low", "normal", "high", IgnoreCase = true)]
    public string Importance { get; } = "normal";

    [Option(Description = "The colors should be red or blue")]
    [RedOrBlue]
    public string Color { get; }

    [Option("--max-size <MB>", Description = "The maximum size of the message in MB.")]
    [Range(1, 50)]
    public int? MaxSize { get; }

    private void OnExecute()
    {
        Console.WriteLine("From = " + From);
        Console.WriteLine("To = " + To);
        Console.WriteLine("Message = " + Message);
        Console.WriteLine("Attachments = " + string.Join(", ", Attachments));
        if (MaxSize.HasValue)
        {
            Console.WriteLine("Max size = " + MaxSize.Value);
        }

        Console.ReadKey();
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

[AttributeUsage(AttributeTargets.Class)]
public class MaxSizeOptionRequiresAttachmentValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        if (value is AttributeProgram obj)
        {
            if (obj.MaxSize.HasValue && (obj.Attachments == null || obj.Attachments?.Length == 0))
            {
                return new ValidationResult("--max-size cannot be used unless --attachments is also specified");
            }
        }
        return ValidationResult.Success;
    }
}
