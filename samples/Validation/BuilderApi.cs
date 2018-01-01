// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

class BuilderApi
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        var optionMessage = app.Option("-m|--message <MSG>", "Required. The message.", CommandOptionType.SingleValue)
            .IsRequired();

        var optionReceiver = app.Option("--to <EMAIL>", "Required. The recipient.", CommandOptionType.SingleValue)
            .IsRequired()
            .Accepts(v => v.IsEmailAddress());

        var optionSender = app.Option("--from <EMAIL>", "Required. The sender.", CommandOptionType.SingleValue)
            .IsRequired()
            .Accepts(v => v.IsEmailAddress());

        var attachments = app.Option("--attachment <FILE>", "Files to attach.", CommandOptionType.MultipleValue)
            .Accepts(v => v.IsExistingFile());

        var optionColor = app.Option("--color <COLOR>", "The color. Should be 'red' or 'blue'.", CommandOptionType.SingleValue);
        optionColor.Validators.Add(new MustBeBlueOrRedValidator());

        app.OnExecute(() =>
        {
            Console.WriteLine("From = " + optionSender.Value());
            Console.WriteLine("To = " + optionReceiver.Value());
            Console.WriteLine("Message = " + optionMessage.Value());
            Console.WriteLine("Attachments = " + string.Join(", ", attachments.Values));
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
