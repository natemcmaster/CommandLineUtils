// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Files here are for simplify annotations of nullable code and are not functional in .NET Standard 2.0
#if NETSTANDARD2_0 || NET46_OR_GREATER
namespace System.Diagnostics.CodeAnalysis
{
    // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.notnullwhenattribute?
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue)
        {
        }
    }

    // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.allownullattribute
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, Inherited=false)]
    internal sealed class AllowNullAttribute : Attribute { }
}
#endif
