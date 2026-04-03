// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Errors;
using McMaster.Extensions.CommandLineUtils.SourceGeneration;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Creates a subcommand for each <see cref="McMaster.Extensions.CommandLineUtils.SubcommandAttribute"/>
    /// on the model type of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class SubcommandAttributeConvention : IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            // MetadataProvider is always available (generated or reflection-based via DefaultMetadataResolver)
            var provider = context.MetadataProvider;
            if (provider == null || context.ModelAccessor == null)
            {
                return;
            }

            foreach (var subMeta in provider.Subcommands)
            {
                AssertSubcommandIsNotCycled(subMeta.SubcommandType, context.Application);

                // Get the subcommand's metadata provider (from factory or registry, with fallback to reflection)
                var subProvider = subMeta.MetadataProviderFactory?.Invoke()
                    ?? DefaultMetadataResolver.Instance.GetProvider(subMeta.SubcommandType);

                // Get the subcommand name
                var name = GetSubcommandName(subMeta.SubcommandType, subProvider);

                // AddSubcommandFromMetadata will call AddSubcommand which validates
                // for duplicate names and throws if necessary
                AddSubcommandFromMetadata(context, subMeta.SubcommandType, subProvider, name);
            }
        }

        private static string GetSubcommandName(Type subcommandType, ICommandMetadataProvider provider)
        {
            var commandInfo = provider.CommandInfo;
            if (!string.IsNullOrEmpty(commandInfo?.Name))
            {
                // Use the explicit name as-is
                return commandInfo.Name;
            }

            // Infer name from type name using CommandNameFromTypeConvention logic
            return CommandNameFromTypeConvention.GetCommandName(subcommandType.Name);
        }

        private void AddSubcommandFromMetadata(
            ConventionContext context,
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
#elif NET472_OR_GREATER
#else
#error Target framework misconfiguration
#endif
            Type subcommandType,
            ICommandMetadataProvider provider,
            string name)
        {
            var commandInfo = provider.CommandInfo;

            // Use reflection to create the proper generic CommandLineApplication<T> type
            // This maintains compatibility with code that expects CommandLineApplication<T>
            var genericType = typeof(CommandLineApplication<>).MakeGenericType(subcommandType);

            // Get the internal constructor: CommandLineApplication<T>(CommandLineApplication parent, string name)
            var constructor = genericType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(CommandLineApplication), typeof(string) },
                null);

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    $"Could not find internal constructor for CommandLineApplication<{subcommandType.Name}>");
            }

            CommandLineApplication subApp;
            try
            {
                subApp = (CommandLineApplication)constructor.Invoke(new object[] { context.Application, name });
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                // Unwrap TargetInvocationException to throw the actual exception
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw; // Unreachable, but required for compiler
            }

            // Register the subcommand with the parent
            context.Application.AddSubcommand(subApp);

            // Apply command metadata using the ApplyTo method which handles all properties
            commandInfo?.ApplyTo(subApp);

            // Note: Do NOT call UseDefaultConventions() here!
            // Conventions are automatically inherited from the parent in the subcommand constructor.
            // Calling UseDefaultConventions() would cause conventions to be applied twice,
            // resulting in duplicate options, arguments, and subcommands.
        }

        private void AssertSubcommandIsNotCycled(Type modelType, CommandLineApplication? parentCommand)
        {
            while (parentCommand != null)
            {
                if (parentCommand is IModelAccessor parentCommandAccessor
                    && parentCommandAccessor.GetModelType() == modelType)
                {
                    throw new SubcommandCycleException(modelType);
                }
                parentCommand = parentCommand.Parent;
            }
        }
    }
}
