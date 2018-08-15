// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Sets the command name based on the model type, if is not otherwise set.
    /// <para>
    /// This attempts to infer a command name using a few rules, such as using kebab-case
    /// and trimming "Command" from the name of the type.
    /// <example>AddCommand => "add"</example>
    /// <example>RemoveItemCommand => "remove-item"</example>
    /// </para>
    /// </summary>
    public class CommandNameFromTypeConvention : IConvention
    {
        /// <inheritdoc />
        public void Apply(ConventionContext context)
        {
            if (!string.IsNullOrEmpty(context.Application.Name))
            {
                return;
            }

            if (context.ModelType == null)
            {
                return;
            }

            var commandName = GetCommandName(context.ModelType.Name);

            if (!string.IsNullOrEmpty(commandName))
            {
                context.Application.Name = commandName;
            }
        }

        internal static string GetCommandName(string typeName)
        {
            const string cmd = "Command";
            if (typeName.Length > cmd.Length && typeName.EndsWith(cmd, StringComparison.Ordinal))
            {
                typeName = typeName.Substring(0, typeName.Length - cmd.Length);
            }

            return typeName.ToKebabCase();
        }
    }
}
