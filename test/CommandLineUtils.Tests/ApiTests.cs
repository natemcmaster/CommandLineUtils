// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class ApiTests
    {
        [Fact]
        public void AllObsoleteMembersAreEditorBrowsableNever()
        {
            foreach (var type in typeof(CommandArgument).Assembly.GetExportedTypes()
                .Where(t => t.IsPublic))
            {
                if (type.GetCustomAttribute<ObsoleteAttribute>() != null)
                {
                    var editorBrowsable = type.GetCustomAttribute<EditorBrowsableAttribute>();
                    Assert.True(editorBrowsable != null, $"Type: {type.FullName} should have [EditorBrowsable]");
                    Assert.True(editorBrowsable.State == EditorBrowsableState.Never,
                        $"Type: {type.FullName} should have EditorBrowsable.State == Never");
                }

                foreach (var member in type.GetMembers(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    if (member.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                        || member.DeclaringType != type)
                    {
                        continue;
                    }

                    if (member.GetCustomAttribute<ObsoleteAttribute>() == null)
                    {
                        continue;
                    }

                    var editorBrowsable = member.GetCustomAttribute<EditorBrowsableAttribute>();
                    Assert.True(editorBrowsable != null,
                        $"{type.FullName}.{member.Name} should have [EditorBrowsable]");
                    Assert.True(editorBrowsable.State == EditorBrowsableState.Never,
                        $"{type.FullName}.{member.Name} should have EditorBrowsable.State == Never");
                }
            }
        }
    }
}
