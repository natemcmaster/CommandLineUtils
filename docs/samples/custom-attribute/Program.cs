// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable CS0649 // this sample uses a field with a setter that runs via reflection

using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;

[MyClassConvention]
public class AttributeConventionProgram
{
    public static int Main(string[] args) => CommandLineApplication.Execute<AttributeConventionProgram>(args);

    [MyFieldConvention]
    private string _workingDirectory;

    private void OnExecute()
    {
        Console.WriteLine("cwd = " + _workingDirectory);
    }
}

//  Custom attributes that apply to the model type should implement IConvention

[AttributeUsage(AttributeTargets.Class)]
internal class MyClassConventionAttribute : Attribute, IConvention
{
    public void Apply(ConventionContext context)
    {
        context.Application.Description = "This command is defined in " + context.ModelType.Assembly.FullName;
    }
}


// Custom attributes that apply to fields, events, methods, and properties should implement IMemberConvention

[AttributeUsage(AttributeTargets.Field)]
internal class MyFieldConventionAttribute : Attribute, IMemberConvention
{
    public void Apply(ConventionContext context, MemberInfo member)
    {
        if (member is FieldInfo field)
        {
            var opt = context.Application.Option("--working-dir", "The working directory", CommandOptionType.SingleOrNoValue);
            opt.DefaultValue = context.Application.WorkingDirectory;
            context.Application.OnParsingComplete(_ =>
            {
                field.SetValue(context.ModelAccessor.GetModel(), opt.Value());
            });
        }
    }
}

#pragma warning restore CS0649
