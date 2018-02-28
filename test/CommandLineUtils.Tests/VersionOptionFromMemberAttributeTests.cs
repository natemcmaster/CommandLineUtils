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
            var builder = new ReflectionAppBuilder<Property>();
            builder.Initialize();
            Assert.Equal("2.0.0", builder.App.ShortVersionGetter());
            Assert.Equal(builder.App.ShortVersionGetter, builder.App.LongVersionGetter);
        }

        [VersionOptionFromMember(MemberName = nameof(Version))]
        private class Method
        {
            private string Version() => "2.0.0";
        }

        [Fact]
        public void GetsVersionInfoFromMethod()
        {
            var builder = new ReflectionAppBuilder<Method>();
            builder.Initialize();
            Assert.Equal("2.0.0", builder.App.ShortVersionGetter());
            Assert.Equal(builder.App.ShortVersionGetter, builder.App.LongVersionGetter);
        }

        [VersionOptionFromMember(MemberName = nameof(Version))]
        private class StaticMethod
        {
            private static string Version() => "2.0.0";
        }

        [Fact]
        public void GetsVersionFromStaticMethod()
        {
            var builder = new ReflectionAppBuilder<StaticMethod>();
            builder.Initialize();
            Assert.Equal("2.0.0", builder.App.ShortVersionGetter());
            Assert.Equal(builder.App.ShortVersionGetter, builder.App.LongVersionGetter);
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
                new ReflectionAppBuilder<MissingMethod>().Initialize();
            });
            Assert.Equal(Strings.NoPropertyOrMethodFound("Version", typeof(MissingMethod)), ex.Message);
        }
    }
}
