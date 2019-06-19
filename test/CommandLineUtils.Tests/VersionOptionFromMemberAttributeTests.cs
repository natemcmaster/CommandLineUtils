// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class VersionOptionFromMemberAttributeTests
    {
        [VersionOptionFromMember(MemberName = nameof(Version))]
        private class Property
        {
            private string Version { get; } = "2.0.0";
        }

        [Fact]
        public void GetsVersionInfoFromProp()
        {
            var app = new CommandLineApplication<Property>();
            app.Conventions.UseVersionOptionFromMemberAttribute();
            Assert.Equal("2.0.0", app.ShortVersionGetter?.Invoke());
            Assert.Equal(app.ShortVersionGetter, app.LongVersionGetter);
        }

        [VersionOptionFromMember(MemberName = nameof(Version))]
        private class Method
        {
            private string Version() => "2.0.0";
        }

        [Fact]
        public void GetsVersionInfoFromMethod()
        {
            var app = new CommandLineApplication<Method>();
            app.Conventions.UseVersionOptionFromMemberAttribute();
            Assert.Equal("2.0.0", app.ShortVersionGetter?.Invoke());
            Assert.Equal(app.ShortVersionGetter, app.LongVersionGetter);
        }

        [VersionOptionFromMember(MemberName = nameof(Version))]
        private class StaticMethod
        {
            private static string Version() => "2.0.0";
        }

        [Fact]
        public void GetsVersionFromStaticMethod()
        {
            var app = new CommandLineApplication<StaticMethod>();
            app.Conventions.UseVersionOptionFromMemberAttribute();
            Assert.Equal("2.0.0", app.ShortVersionGetter?.Invoke());
            Assert.Equal(app.ShortVersionGetter, app.LongVersionGetter);
        }

        [VersionOptionFromMember(MemberName = "Version")]
        private class MissingMethod
        {
        }

        [Fact]
        public void ThrowWhenMemberDoesNotExist()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                new CommandLineApplication<MissingMethod>().Conventions.UseVersionOptionFromMemberAttribute();
            });
            Assert.Equal(Strings.NoPropertyOrMethodFound("Version", typeof(MissingMethod)), ex.Message);
        }
    }
}
