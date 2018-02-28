// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Conventions;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Methods for adding commonly used conventions
    /// </summary>
    public static class ConventionBuilderExtensions
    {
        /// <summary>
        /// Applies a collection of default conventions, such as applying options in attributes
        /// on the model type,
        /// </summary>
        /// <returns>The builder.</returns>
        public static IConventionBuilder UseDefaultConventions(this IConventionBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder
                .SetRemainingArgsProperty()
                .UseCommandAttribute()
                .SetAppNameFromEntryAssembly()
                .SetSubcommandProperty()
                .SetParentProperty()
                .UseVersionOptionFromMemberAttribute();
        }

        /// <summary>
        /// Sets a property named "RemainingArgs" or "RemainingArguments" on the model type to the value
        /// of <see cref="CommandLineApplication.RemainingArguments" />.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static IConventionBuilder SetRemainingArgsProperty(this IConventionBuilder builder)
            => builder.AddConvention(new RemainingArgsPropertyConvention());

        /// <summary>
        /// Sets a property named "Subcommand" on the model type to the value
        /// of the model of the selected subcommand.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static IConventionBuilder SetSubcommandProperty(this IConventionBuilder builder)
            => builder.AddConvention(new SubcommandPropertyConvention());

        /// <summary>
        /// Sets a property named "Parent" on the model type to the value
        /// of the model of the parent command.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static IConventionBuilder SetParentProperty(this IConventionBuilder builder)
            => builder.AddConvention(new ParentPropertyConvention());

        /// <summary>
        /// Sets <see cref="CommandLineApplication.Name" /> to match the name of
        /// <see cref="System.Reflection.Assembly.GetEntryAssembly" />
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static IConventionBuilder SetAppNameFromEntryAssembly(this IConventionBuilder builder)
            => builder.AddConvention(new AppNameFromEntryAssemblyConvention());

        /// <summary>
        /// Applies settings from <see cref="CommandAttribute" /> on the model type.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static IConventionBuilder UseCommandAttribute(this IConventionBuilder builder)
            => builder.AddConvention(new CommandAttributeConvention());

        /// <summary>
        /// Applies settings from <see cref="VersionOptionFromMemberAttribute" /> on the model type.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static IConventionBuilder UseVersionOptionFromMemberAttribute(this IConventionBuilder builder)
            => builder.AddConvention(new VersionOptionFromMemberAttributeConvention());
    }
}
