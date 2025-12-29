// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data for [HelpOption] attribute.
    /// </summary>
    internal sealed class HelpOptionData
    {
        public string? PropertyName { get; set; }
        public string? Template { get; set; }
        public string? Description { get; set; }
        public bool? Inherited { get; set; }
    }
}
