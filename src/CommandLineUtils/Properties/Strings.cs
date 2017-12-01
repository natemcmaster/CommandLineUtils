// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils
{
    internal static class Strings
    {
        public const string DefaultHelpTemplate = "-?|-h|--help";
        public const string DefaultHelpOptionDescription = "Show help information";

        public const string DefaultVersionTemplate = "--version";
        public const string DefaultVersionOptionDescription = "Show version information";

        public const string NoValueTypesMustBeBoolean
            = "Cannot specify CommandOptionType.NoValue unless the type is boolean.";

        public const string AmbiguousOnExecuteMethod
            = "Could not determine which 'OnExecute' or 'OnExecuteAsync' method to use. Multiple methods with this name were found";

        public const string NoOnExecuteMethodFound
            = "No method named 'OnExecute' or 'OnExecuteAsync' could be found";

        public static string InvalidOnExecuteReturnType(string methodName)
            => methodName + " must have a return type of int or void, or if the method is async, Task<int> or Task.";

        public static string CannotDetermineOptionType(PropertyInfo member)
            => $"Could not automatically determine the {nameof(CommandOptionType)} for type {member.PropertyType.FullName}. " +
                    $"Set the {nameof(OptionAttribute.OptionType)} on the {nameof(OptionAttribute)} declaration for {member.DeclaringType.FullName}.{member.Name}.";

        public static string OptionNameIsAmbiguous(string optionName, PropertyInfo first, PropertyInfo second)
            => $"Ambiguous option name. Both {first.DeclaringType.FullName}.{first.Name} and {second.DeclaringType.FullName}.{second.Name} produce a CommandOption with the name '{optionName}'";

        public static string BothOptionAndArgumentAttributesCannotBeSpecified(PropertyInfo prop)
            => $"Cannot specify both {nameof(OptionAttribute)} and {nameof(ArgumentAttribute)} on a property {prop.DeclaringType.Name}.{prop.Name}.";

        public static string BothOptionAndHelpOptionAttributesCannotBeSpecified(PropertyInfo prop)
            => $"Cannot specify both {nameof(OptionAttribute)} and {nameof(HelpOptionAttribute)} on a property {prop.DeclaringType.Name}.{prop.Name}.";

        public static string BothOptionAndVersionOptionAttributesCannotBeSpecified(PropertyInfo prop)
            => $"Cannot specify both {nameof(OptionAttribute)} and {nameof(VersionOptionAttribute)} on a property {prop.DeclaringType.Name}.{prop.Name}.";

        internal static string UnsupportedParameterTypeOnMethod(string methodName, ParameterInfo methodParam)
            => $"Unsupported type on {methodName} '{methodParam.ParameterType.FullName}' on parameter {methodParam.Name}";

        public static string BothHelpOptionAndVersionOptionAttributesCannotBeSpecified(PropertyInfo prop)
            => $"Cannot specify both {nameof(HelpOptionAttribute)} and {nameof(VersionOptionAttribute)} on a property {prop.DeclaringType.Name}.{prop.Name}.";

        public static string DuplicateArgumentPosition(int order, PropertyInfo first, PropertyInfo second)
            => $"Duplicate value for argument order. Both {first.DeclaringType.FullName}.{first.Name} and {second.DeclaringType.FullName}.{second.Name} have set Order = {order}";

        public static string OnlyLastArgumentCanAllowMultipleValues(string lastArgName)
            => $"The last argument '{lastArgName}' accepts multiple values. No more argument can be added.";

        public static string CannotDetermineParserType(PropertyInfo prop)
            => $"Could not automatically determine how to convert string values into {prop.PropertyType.FullName}.";

        public static string MultipleValuesArgumentShouldBeCollection
            = "ArgumentAttribute.MultipleValues should be true if the property type is an array or collection.";

        public const string HelpOptionOnTypeAndProperty
            = "Multiple HelpOptionAttributes found. HelpOptionAttribute should only be used one per type, either on one property or on the type.";

        public const string MultipleHelpOptionPropertiesFound
            = "Multiple HelpOptionAttributes found. HelpOptionAttribute should only be used on one property per type.";

        public const string VersionOptionOnTypeAndProperty
            = "Multiple VersionOptionAttributes found. VersionOptionAttribute should only be used one per type, either on one property or on the type.";

        public const string MultipleVersionOptionPropertiesFound
            = "Multiple VersionOptionAttributes found. VersionOptionAttribute should only be used on one property per type.";

        public static string RemainingArgsPropsIsUnassignable(TypeInfo typeInfo)
            => $"The RemainingArguments property type on {typeInfo.Name} is invalid. It must be assignable from string[].";
    }
}
