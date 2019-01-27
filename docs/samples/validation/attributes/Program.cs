// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

[RequiredDependentsValidation(nameof(Attachments), nameof(MaxSize))]
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
public class RequiredDependentsValidationAttribute : ValidationAttribute
{

    public string[] DependentProperties { get; set; }

    public string[] Properties { get; set; }

    public RequiredDependentsValidationAttribute(string dependentProperty, params string[] properties)
        : this(new string[] { dependentProperty } ?? throw new ArgumentException(nameof(dependentProperty)), properties) { }


    public RequiredDependentsValidationAttribute(string[] dependentProperties, string[] properties)
        : base()
    {
        switch (dependentProperties)
        {
            case null:
                throw new ArgumentNullException(nameof(dependentProperties));
            case var a when a.Length is 0:
            case var b when b.Length > 0 && b.All(String.IsNullOrWhiteSpace):
                throw new ArgumentException(nameof(dependentProperties));
            default:
                DependentProperties = dependentProperties;
                break;
        }

        switch (properties)
        {
            case null:
                throw new ArgumentNullException(nameof(properties));
            case var p when p.Length is 0:
                throw new ArgumentException(nameof(properties));
            default:
                Properties = properties;
                break;
        }
    }

    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var type = value.GetType();

        var dependents = type.GetProperties().Join(DependentProperties, pi => pi.Name, s => s, ((info, s) => info)).ToArray();
        var properties = type.GetProperties().Join(Properties, pi => pi.Name, s => s, ((info, s) => info)).ToArray();

        if (dependents?.Count() == 0 || properties?.Count() == 0)
        {
            return new ValidationResult(ErrorMessage);
        }

        var checkProps = false;

        foreach (var dependent in dependents)
        {
            checkProps = dependent.GetValue(value) != null;
            if (checkProps)
            {
                break;
            }
        }

        if (!checkProps)
        {
            return ValidationResult.Success;
        }

        foreach (var property in properties)
        {
            if (property.GetValue(value) is null)
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}
