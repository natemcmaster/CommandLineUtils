// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using McMaster.Extensions.CommandLineUtils.Conventions;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Suppress <see cref="DefaultHelpOptionConvention"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true)]
    public sealed class SuppressDefaultHelpOptionAttribute : Attribute
    {
    }
}
