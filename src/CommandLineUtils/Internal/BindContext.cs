// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class BindContext
    {
        public CommandLineApplication App { get; set; }
        public object Target { get; set; }
        public BindContext Child { get; set; }
        public ValidationResult ValidationResult { get; set; }

        public BindContext GetBottomContext()
        {
            var retVal = this;
            while (retVal?.Child != null)
            {
                retVal = retVal.Child;
            }
            return retVal;
        }
    }
}
