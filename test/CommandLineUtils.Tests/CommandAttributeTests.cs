// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
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

        #region Name Property Tests

        [Fact]
        public void Name_SetToNull_ClearsNames()
        {
            // This tests lines 57-59: when Name is set to null, _names becomes empty
            var attr = new CommandAttribute("initial-name");
            Assert.Equal("initial-name", attr.Name);

            attr.Name = null;

            Assert.Null(attr.Name);
            Assert.Empty(attr.Names);
        }

        [Fact]
        public void Name_SetToValue_CreatesSingleElementArray()
        {
            var attr = new CommandAttribute();
            Assert.Null(attr.Name);

            attr.Name = "test-name";

            Assert.Equal("test-name", attr.Name);
            Assert.Single(attr.Names);
        }

        [Fact]
        public void Names_WithMultipleNames_FirstIsName()
        {
            var attr = new CommandAttribute("primary", "alias1", "alias2");

            Assert.Equal("primary", attr.Name);
            Assert.Equal(new[] { "primary", "alias1", "alias2" }, attr.Names);
        }

        [Fact]
        public void Name_Getter_ReturnsNull_WhenNamesEmpty()
        {
            var attr = new CommandAttribute();

            Assert.Null(attr.Name);
            Assert.Empty(attr.Names);
        }

        #endregion

        #region Configure Method Tests

        [Command("cmd", "alias1", "alias2")]
        private class CommandWithAliases
        { }

        [Fact]
        public void Configure_AddsAliases_FromNames()
        {
            // This tests lines 179-182: foreach loop adding aliases
            var app = new CommandLineApplication<CommandWithAliases>();
            app.Conventions.UseCommandAttribute();

            Assert.Equal("cmd", app.Name);
            Assert.Contains("alias1", app.Names);
            Assert.Contains("alias2", app.Names);
        }

        [Command(
            Name = "full-test",
            FullName = "Full Test Command",
            Description = "A test description",
            ExtendedHelpText = "Extended help here",
            ShowInHelpText = false,
            AllowArgumentSeparator = true,
            ResponseFileHandling = ResponseFileHandling.ParseArgsAsSpaceSeparated,
            OptionsComparison = StringComparison.OrdinalIgnoreCase,
            UsePagerForHelpText = true)]
        private class CommandWithAllProperties
        { }

        [Fact]
        public void Configure_SetsAllProperties()
        {
            // This tests lines 184-192 in Configure method
            var app = new CommandLineApplication<CommandWithAllProperties>();
            app.Conventions.UseCommandAttribute();

            Assert.Equal("full-test", app.Name);
            Assert.Equal("Full Test Command", app.FullName);
            Assert.Equal("A test description", app.Description);
            Assert.Equal("Extended help here", app.ExtendedHelpText);
            Assert.False(app.ShowInHelpText);
            Assert.True(app.AllowArgumentSeparator);
            Assert.Equal(ResponseFileHandling.ParseArgsAsSpaceSeparated, app.ResponseFileHandling);
            Assert.Equal(StringComparison.OrdinalIgnoreCase, app.OptionsComparison);
            Assert.True(app.UsePagerForHelpText);
        }

        [Command]
        private class CommandWithNoName
        { }

        [Fact]
        public void Configure_PreservesExistingName_WhenAttributeNameIsNull()
        {
            // This tests line 177: app.Name = Name ?? app.Name
            var app = new CommandLineApplication<CommandWithNoName>();
            app.Name = "existing-name";
            app.Conventions.UseCommandAttribute();

            Assert.Equal("existing-name", app.Name);
        }

        [Command("override-name")]
        private class CommandWithName
        { }

        [Fact]
        public void Configure_OverridesExistingName_WhenAttributeNameIsSet()
        {
            var app = new CommandLineApplication<CommandWithName>();
            app.Name = "original-name";
            app.Conventions.UseCommandAttribute();

            Assert.Equal("override-name", app.Name);
        }

        [Fact]
        public void Configure_SetsParseCulture()
        {
            // Test line 191: app.ValueParsers.ParseCulture = ParseCulture
            var app = new CommandLineApplication<CommandWithAllProperties>();
            app.Conventions.UseCommandAttribute();

            // The default ParseCulture is CurrentCulture
            Assert.Equal(System.Globalization.CultureInfo.CurrentCulture, app.ValueParsers.ParseCulture);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_Default_HasEmptyNames()
        {
            var attr = new CommandAttribute();

            Assert.Null(attr.Name);
            Assert.Empty(attr.Names);
        }

        [Fact]
        public void Constructor_WithSingleName_SetsName()
        {
            var attr = new CommandAttribute("single");

            Assert.Equal("single", attr.Name);
            Assert.Single(attr.Names);
        }

        [Fact]
        public void Constructor_WithMultipleNames_SetsAllNames()
        {
            var attr = new CommandAttribute("primary", "secondary", "tertiary");

            Assert.Equal("primary", attr.Name);
            Assert.Equal(3, attr.Names.Count());
        }

        #endregion

        #region Configure Method Direct Tests

        [Fact]
        public void Configure_Direct_SetsAllProperties()
        {
            // Directly test the internal Configure method
            var attr = new CommandAttribute("test-cmd")
            {
                FullName = "Test Command Full Name",
                Description = "Test description",
                ExtendedHelpText = "Extended help text",
                ShowInHelpText = false,
                AllowArgumentSeparator = true,
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
                OptionsComparison = StringComparison.OrdinalIgnoreCase,
                UsePagerForHelpText = true
            };

            var app = new CommandLineApplication();
            attr.Configure(app);

            Assert.Equal("test-cmd", app.Name);
            Assert.Equal("Test Command Full Name", app.FullName);
            Assert.Equal("Test description", app.Description);
            Assert.Equal("Extended help text", app.ExtendedHelpText);
            Assert.False(app.ShowInHelpText);
            Assert.True(app.AllowArgumentSeparator);
            Assert.Equal(ResponseFileHandling.ParseArgsAsLineSeparated, app.ResponseFileHandling);
            Assert.Equal(StringComparison.OrdinalIgnoreCase, app.OptionsComparison);
            Assert.True(app.UsePagerForHelpText);
        }

        [Fact]
        public void Configure_Direct_WithAliases_AddsAllNames()
        {
            // Test lines 179-182: foreach loop for aliases
            var attr = new CommandAttribute("primary", "alias1", "alias2");

            var app = new CommandLineApplication();
            attr.Configure(app);

            Assert.Equal("primary", app.Name);
            Assert.Contains("alias1", app.Names);
            Assert.Contains("alias2", app.Names);
            Assert.Equal(3, app.Names.Count());
        }

        [Fact]
        public void Configure_Direct_WithNullName_PreservesExistingName()
        {
            // Test line 177: app.Name = Name ?? app.Name
            var attr = new CommandAttribute();
            Assert.Null(attr.Name);

            var app = new CommandLineApplication { Name = "existing" };
            attr.Configure(app);

            Assert.Equal("existing", app.Name);
        }

        [Fact]
        public void Configure_Direct_WithClusterOptionsSet_SetsValue()
        {
            // Test lines 194-197
            var attr = new CommandAttribute { ClusterOptions = false };

            var app = new CommandLineApplication { ClusterOptions = true };
            attr.Configure(app);

            Assert.False(app.ClusterOptions);
        }

        [Fact]
        public void Configure_Direct_WithClusterOptionsNotSet_PreservesDefault()
        {
            // Test lines 194-197 (branch not taken)
            var attr = new CommandAttribute();
            Assert.False(attr.ClusterOptionsWasSet);

            var app = new CommandLineApplication { ClusterOptions = false };
            attr.Configure(app);

            // Should remain false because ClusterOptions wasn't explicitly set
            Assert.False(app.ClusterOptions);
        }

        [Fact]
        public void Configure_Direct_WithUnrecognizedArgumentHandlingSet_SetsValue()
        {
            // Test lines 199-202
            var attr = new CommandAttribute
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect
            };

            var app = new CommandLineApplication();
            attr.Configure(app);

            Assert.Equal(UnrecognizedArgumentHandling.StopParsingAndCollect, app.UnrecognizedArgumentHandling);
        }

        [Fact]
        public void Configure_Direct_WithUnrecognizedArgumentHandlingNotSet_PreservesDefault()
        {
            // Test lines 199-202 (branch not taken)
            var attr = new CommandAttribute();
            Assert.False(attr.UnrecognizedArgumentHandlingWasSet);

            var app = new CommandLineApplication
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue
            };
            attr.Configure(app);

            // Should remain CollectAndContinue because UnrecognizedArgumentHandling wasn't explicitly set
            Assert.Equal(UnrecognizedArgumentHandling.CollectAndContinue, app.UnrecognizedArgumentHandling);
        }

        [Fact]
        public void Configure_Direct_SetsParseCulture()
        {
            // Test line 191
            var attr = new CommandAttribute
            {
                ParseCulture = System.Globalization.CultureInfo.InvariantCulture
            };

            var app = new CommandLineApplication();
            attr.Configure(app);

            Assert.Equal(System.Globalization.CultureInfo.InvariantCulture, app.ValueParsers.ParseCulture);
        }

        [Fact]
        public void Configure_Direct_NoAliases_DoesNotAddNames()
        {
            // Test lines 179-182 with empty iteration
            var attr = new CommandAttribute("single-name");

            var app = new CommandLineApplication();
            attr.Configure(app);

            Assert.Equal("single-name", app.Name);
            Assert.Single(app.Names);
        }

        #endregion
    }
}
