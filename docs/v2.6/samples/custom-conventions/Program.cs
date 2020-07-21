// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;

/// <summary>
/// This custom convention adds a subcommand for each method named "Handle-something"
/// </summary>
public class MethodsAsSubcommandsConventions : IConvention
{
    public void Apply(ConventionContext context)
    {
        const string prefix = "Handle";

        if (context.ModelType == null)
        {
            // This would happen if the convention were applied to CommandLineApplication.
            // Conventions are applied by all created subcommands, so we need this check.
            return;
        }

        // Use reflection to find "Handle" methods.
        var handleMethods = context.ModelType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name.StartsWith(prefix, StringComparison.Ordinal));

        foreach (var method in handleMethods)
        {
            var subcommandName = method.Name.Substring(prefix.Length).ToLowerInvariant();
            // Create a subcommand for each.
            context.Application.Command(subcommandName, cmd =>
            {
                // Translate the method parameters into arguments and options
                var parameters = method.GetParameters();

                // define a matching array of functions that will produce the values to pass into the method
                var paramValueFactories = new Func<object>[parameters.Length];

                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];

                    // treat method parameters with default values as options
                    if (param.HasDefaultValue)
                    {
                        // if command type == bool, make it a switch. Otherwise, accept a single value.
                        var type = param.ParameterType == typeof(bool)
                            ? CommandOptionType.NoValue
                            : CommandOptionType.SingleValue;

                        var template = "--" + param.Name.ToLowerInvariant();
                        var opt = cmd.Option(template, "", type);
                        paramValueFactories[i] = () =>
                        {
                            if (param.ParameterType == typeof(bool)) return opt.HasValue();
                            if (!opt.HasValue()) return param.DefaultValue;
                            return opt.Value();
                        };
                    }
                    else
                    {
                        // otherwise it is a required argument.
                        var arg = cmd.Argument(param.Name, "").IsRequired();
                        paramValueFactories[i] = () => arg.Value;
                    }
                }

                // when this subcommand is selected, invoke the method on the model instance
                cmd.OnExecuteAsync(async cancellationToken =>
                {
                    // get an instance of the model type from CommandLineApplication<TModel>
                    var modelInstance = context.ModelAccessor.GetModel();
                    var methodArgs = paramValueFactories.Select(f => f.Invoke()).ToArray();
                    var value = method.Invoke(modelInstance, methodArgs);
                    if (value is int retVal) return retVal;
                    if (value is Task<int> retTask) return await retTask;
                    if (value is Task retWait) await retWait;
                    return 0;
                });
            });
        }
    }
}

public class MethodsAsSubcommandsProgram
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication<MethodsAsSubcommandsProgram>();
        app.HelpOption(inherited: true);
        app.Conventions
            .AddConvention(new MethodsAsSubcommandsConventions())
            .SetAppNameFromEntryAssembly();
        return app.Execute(args);
    }

    // subcommand "add"
    private async Task HandleAdd(string name, string version = "(default version)")
    {
        Console.WriteLine($"Adding {name} {version}...");
        await Task.Delay(2_000);
        Console.WriteLine("Done");
    }

    // subcommand "list"
    private void HandleList()
    {
        Console.WriteLine(@"
- one
- two
- three");
    }

    // subcommand "remove"
    private void HandleRemove(string name, bool force = false)
    {
        if (force)
        {
            Console.WriteLine($"Forcing removal of {name}");
        }
        else
        {
            Console.WriteLine($"Removing {name}");
        }
    }
}
