// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data for [VersionOption] attribute.
    /// </summary>
    internal sealed class VersionOptionData
    {
        public string? PropertyName { get; set; }
        public string? Template { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
    }
}
