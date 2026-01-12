// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data for [VersionOption] or [VersionOptionFromMember] attribute.
    /// </summary>
    internal sealed class VersionOptionData
    {
        public string? PropertyName { get; set; }
        public string? Template { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        /// <summary>
        /// For [VersionOptionFromMember]: the name of the method or property that returns the version.
        /// </summary>
        public string? MemberName { get; set; }
    }
}
