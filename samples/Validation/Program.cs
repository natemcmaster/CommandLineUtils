// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

class Program
{
    static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        var optionMessage = app.Option("-m|--message <MSG>", "Required. The message.", CommandOptionType.SingleValue)
            .IsRequired();

        var optionReceiver = app.Option("--to <EMAIL>", "Required. The recipient.", CommandOptionType.SingleValue)
            .IsRequired();

        var optionSender = app.Option("--from <EMAIL>", "Required. The sender.", CommandOptionType.SingleValue)
            .IsRequired();

        var optionColor = app.Option("--color <COLOR>", "The color. Should be 'red' or 'blue'.", CommandOptionType.SingleValue);
        optionColor.Validators.Add(new MustBeBlueOrRedValidator());

        app.OnExecute(() =>
        {
            Console.WriteLine("From = " + optionSender.Value());
            Console.WriteLine("To = " + optionReceiver.Value());
            Console.WriteLine("Message = " + optionMessage.Value());
        });

        return app.Execute(args);
    }
}

class MustBeBlueOrRedValidator : IOptionValidator
{
    public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
    {
        // This validator only runs if there is a value
        if (!option.HasValue()) return ValidationResult.Success;
        var val = option.Value();

        if (val != "red" && val != "blue")
        {
            return new ValidationResult($"The value for --{option.LongName} must be 'red' or 'blue'");
        }

        return ValidationResult.Success;
    }
}
