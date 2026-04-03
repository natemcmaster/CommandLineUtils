// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils.Generators
{
    /// <summary>
    /// Data from [Command] attribute.
    /// </summary>
    internal sealed class CommandAttributeData
    {
        public string? Name { get; set; }
        public string[]? AdditionalNames { get; set; }
        public string? Description { get; set; }
        public string? FullName { get; set; }
        public string? ExtendedHelpText { get; set; }
        public bool? ShowInHelpText { get; set; }
        public bool? AllowArgumentSeparator { get; set; }
        public bool? ClusterOptions { get; set; }
        public bool? UsePagerForHelpText { get; set; }
        public int? ResponseFileHandling { get; set; }
        public int? OptionsComparison { get; set; }
        public int? UnrecognizedArgumentHandling { get; set; }
    }
}
