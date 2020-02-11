// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandAttributeTests
    {
        [Command(
            ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
            AllowArgumentSeparator = true,
            UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
        private class ParsingOptions
        { }

        [Fact]
        public void HandlesParsingOptionsAttribute()
        {
            var app = new CommandLineApplication<ParsingOptions>();
            app.Conventions.UseCommandAttribute();

            Assert.Equal(ResponseFileHandling.ParseArgsAsLineSeparated, app.ResponseFileHandling);
            Assert.Equal(UnrecognizedArgumentHandling.StopParsingAndCollect, app.UnrecognizedArgumentHandling);
            Assert.True(app.AllowArgumentSeparator);
        }
    }
}
