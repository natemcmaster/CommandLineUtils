﻿// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.Extensions.CommandLineUtils
{
    internal static class Strings
    {
        public const string DefaultHelpTemplate = "-?|-h|--help";
        public const string DefaultHelpOptionDescription = "Show help information.";

        public const string DefaultVersionTemplate = "--version";
        public const string DefaultVersionOptionDescription = "Show version information.";

        public const string IsEmptyArray = "value is an empty array.";

        public const string PathMustNotBeRelative = "File path must not be relative.";

        public const string NoValueTypesMustBeBoolean
            = "Cannot specify CommandOptionType.NoValue unless the type is boolean.";

        public const string AmbiguousOnExecuteMethod
            = "Could not determine which 'OnExecute' or 'OnExecuteAsync' method to use. Multiple methods with this name were found.";

        public const string NoOnExecuteMethodFound
            = "No method named 'OnExecute' or 'OnExecuteAsync' could be found.";

        public const string ConventionRequiresModel
            = "This convention cannot be used on a command that does not implement " + nameof(IModelAccessor) + ".";

        public static string InvalidOnExecuteReturnType(string methodName)
            => methodName + " must have a return type of int or void, or if the method is async, Task<int> or Task.";

        public static string InvalidOnValidateReturnType(Type modelType)
            => $"The OnValidate method on {modelType.FullName} must return {typeof(ValidationResult).FullName}.";

        public static string CannotDetermineOptionType(PropertyInfo member)
            => $"Could not automatically determine the {nameof(CommandOptionType)} for type {member.PropertyType.FullName}. " +
                    $"Set the {nameof(OptionAttribute.OptionType)} on the {nameof(OptionAttribute)} declaration for {member.DeclaringType?.FullName}.{member.Name}.";

        public static string OptionNameIsAmbiguous(string optionName, PropertyInfo first, PropertyInfo second)
            => $"Ambiguous option name. Both {first.DeclaringType?.FullName}.{first.Name} and {second.DeclaringType?.FullName}.{second.Name} produce a CommandOption with the name '{optionName}'.";

        public static string DuplicateSubcommandName(string commandName)
            => $"The subcommand name '{commandName}' has already been been specified. Subcommand names must be unique.";

        public static string BothOptionAndArgumentAttributesCannotBeSpecified(PropertyInfo prop)
            => $"Cannot specify both {nameof(OptionAttribute)} and {nameof(ArgumentAttribute)} on property {prop.DeclaringType?.Name}.{prop.Name}.";

        public static string BothOptionAndHelpOptionAttributesCannotBeSpecified(PropertyInfo prop)
            => $"Cannot specify both {nameof(OptionAttribute)} and {nameof(HelpOptionAttribute)} on property {prop.DeclaringType?.Name}.{prop.Name}.";

        public static string BothOptionAndVersionOptionAttributesCannotBeSpecified(PropertyInfo prop)
            => $"Cannot specify both {nameof(OptionAttribute)} and {nameof(VersionOptionAttribute)} on property {prop.DeclaringType?.Name}.{prop.Name}.";

        internal static string UnsupportedParameterTypeOnMethod(string methodName, ParameterInfo methodParam)
            => $"Unsupported type on {methodName} '{methodParam.ParameterType.FullName}' on parameter {methodParam.Name}.";

        public static string BothHelpOptionAndVersionOptionAttributesCannotBeSpecified(PropertyInfo prop)
            => $"Cannot specify both {nameof(HelpOptionAttribute)} and {nameof(VersionOptionAttribute)} on property {prop.DeclaringType?.Name}.{prop.Name}.";

        public static string DuplicateArgumentPosition(int order, PropertyInfo first, PropertyInfo second)
            => $"Duplicate value for argument order. Both {first.DeclaringType?.FullName}.{first.Name} and {second.DeclaringType?.FullName}.{second.Name} have set Order = {order}.";

        public static string OnlyLastArgumentCanAllowMultipleValues(string? lastArgName)
            => $"The last argument '{lastArgName}' accepts multiple values. No more argument can be added.";

        public static string CannotDetermineParserType(Type type)
            => $"Could not automatically determine how to convert string values into {type.FullName}.";

        public static string CannotDetermineParserType(PropertyInfo prop)
            => $"Could not automatically determine how to convert string values into {prop.PropertyType.FullName} on property {prop.DeclaringType?.Name}.{prop.Name}.";

        public const string HelpOptionOnTypeAndProperty
            = "Multiple HelpOptionAttributes found. HelpOptionAttribute should only be used one per type, either on one property or on the type.";

        public const string MultipleHelpOptionPropertiesFound
            = "Multiple HelpOptionAttributes found. HelpOptionAttribute should only be used on one property per type.";

        public const string VersionOptionOnTypeAndProperty
            = "Multiple VersionOptionAttributes found. VersionOptionAttribute should only be used one per type, either on one property or on the type.";

        public const string MultipleVersionOptionPropertiesFound
            = "Multiple VersionOptionAttributes found. VersionOptionAttribute should only be used on one property per type.";

        public static string RemainingArgsPropsIsUnassignable(Type type)
            => $"The RemainingArguments property type on {type.Name} is invalid. It must be assignable from string[].";

        public static string NoPropertyOrMethodFound(string memberName, Type type)
            => $"Could not find a property or method named {memberName} on type {type.FullName}.";

        public static string NoParameterTypeRegistered(Type modelType, Type paramType)
            => $"The constructor of type '{modelType}' contains the parameter of type '{paramType}' is not registered, Ensure the type '{paramType}' are registered in additional services with CommandLineApplication.Conventions.UseConstructorInjection(IServiceProvider additionalServices).";

        public static string NoAnyPublicConstructorFound(Type modelType)
            => $"Could not find any public constructors of type '{modelType}'.";

        public static string NoMatchedConstructorFound(Type modelType)
            => $"Could not found any matched constructors of type '{modelType}'.";
    }
}
