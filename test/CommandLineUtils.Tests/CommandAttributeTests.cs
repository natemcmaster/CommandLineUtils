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

        #region ClusterOptionsWasSet and UnrecognizedArgumentHandlingWasSet Tests

        [Command(ClusterOptions = true)]
        private class ClusterOptionsExplicitlySet
        { }

        [Command]
        private class ClusterOptionsNotSet
        { }

        [Fact]
        public void ClusterOptionsWasSet_ReturnsTrue_WhenExplicitlySet()
        {
            var attr = new CommandAttribute { ClusterOptions = true };

            Assert.True(attr.ClusterOptionsWasSet);
        }

        [Fact]
        public void ClusterOptionsWasSet_ReturnsFalse_WhenNotSet()
        {
            var attr = new CommandAttribute();

            Assert.False(attr.ClusterOptionsWasSet);
        }

        [Fact]
        public void Configure_SetsClusterOptions_WhenExplicitlySet()
        {
            var app = new CommandLineApplication<ClusterOptionsExplicitlySet>();
            // Set it to false first so we can verify it gets changed
            app.ClusterOptions = false;
            app.Conventions.UseCommandAttribute();

            Assert.True(app.ClusterOptions);
        }

        [Fact]
        public void Configure_DoesNotSetClusterOptions_WhenNotExplicitlySet()
        {
            var app = new CommandLineApplication<ClusterOptionsNotSet>();
            // Set it to false first - it should remain unchanged if not explicitly set
            app.ClusterOptions = false;
            app.Conventions.UseCommandAttribute();

            Assert.False(app.ClusterOptions);
        }

        [Command(UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue)]
        private class UnrecognizedArgumentHandlingExplicitlySet
        { }

        [Command]
        private class UnrecognizedArgumentHandlingNotSet
        { }

        [Fact]
        public void UnrecognizedArgumentHandlingWasSet_ReturnsTrue_WhenExplicitlySet()
        {
            var attr = new CommandAttribute { UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue };

            Assert.True(attr.UnrecognizedArgumentHandlingWasSet);
        }

        [Fact]
        public void UnrecognizedArgumentHandlingWasSet_ReturnsFalse_WhenNotSet()
        {
            var attr = new CommandAttribute();

            Assert.False(attr.UnrecognizedArgumentHandlingWasSet);
        }

        [Fact]
        public void Configure_SetsUnrecognizedArgumentHandling_WhenExplicitlySet()
        {
            var app = new CommandLineApplication<UnrecognizedArgumentHandlingExplicitlySet>();
            app.Conventions.UseCommandAttribute();

            Assert.Equal(UnrecognizedArgumentHandling.CollectAndContinue, app.UnrecognizedArgumentHandling);
        }

        [Fact]
        public void Configure_DoesNotSetUnrecognizedArgumentHandling_WhenNotExplicitlySet()
        {
            var app = new CommandLineApplication<UnrecognizedArgumentHandlingNotSet>();
            // Set to a specific value - it should remain unchanged if not explicitly set
            app.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect;
            app.Conventions.UseCommandAttribute();

            // Default when not set via attribute should be Throw (per the getter default)
            // But since we're not calling UseDefaultConventions, the app's value should be preserved
            Assert.Equal(UnrecognizedArgumentHandling.StopParsingAndCollect, app.UnrecognizedArgumentHandling);
        }

        [Fact]
        public void ClusterOptions_DefaultsToTrue()
        {
            var attr = new CommandAttribute();

            Assert.True(attr.ClusterOptions);
        }

        [Fact]
        public void UnrecognizedArgumentHandling_DefaultsToThrow()
        {
            var attr = new CommandAttribute();

            Assert.Equal(UnrecognizedArgumentHandling.Throw, attr.UnrecognizedArgumentHandling);
        }

        #endregion
    }
}
