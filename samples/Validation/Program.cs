// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using McMaster.Extensions.CommandLineUtils;

class Program
{
    static int Main(string[] args)
    {
        var sample = Prompt.GetInt(@"Which sample?
1 - Attributes
2 - Builder API
> ");
        switch (sample)
        {
            case 1:
                return AttributeProgram.Main(args);
            case 2:
                return BuilderApi.Main(args);
        }

        return 1;
    }
}
