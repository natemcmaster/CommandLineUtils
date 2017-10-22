// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class CommandOptionTypeMapperTests
    {
        [Theory]
        [InlineData(typeof(bool), CommandOptionType.NoValue)]
        [InlineData(typeof(bool?), CommandOptionType.NoValue)]
        [InlineData(typeof(byte), CommandOptionType.SingleValue)]
        [InlineData(typeof(short), CommandOptionType.SingleValue)]
        [InlineData(typeof(int), CommandOptionType.SingleValue)]
        [InlineData(typeof(long), CommandOptionType.SingleValue)]
        [InlineData(typeof(uint), CommandOptionType.SingleValue)]
        [InlineData(typeof(ulong), CommandOptionType.SingleValue)]
        [InlineData(typeof(ushort), CommandOptionType.SingleValue)]
        [InlineData(typeof(byte?), CommandOptionType.SingleValue)]
        [InlineData(typeof(short?), CommandOptionType.SingleValue)]
        [InlineData(typeof(int?), CommandOptionType.SingleValue)]
        [InlineData(typeof(long?), CommandOptionType.SingleValue)]
        [InlineData(typeof(ushort?), CommandOptionType.SingleValue)]
        [InlineData(typeof(uint?), CommandOptionType.SingleValue)]
        [InlineData(typeof(ulong?), CommandOptionType.SingleValue)]
        [InlineData(typeof(string), CommandOptionType.SingleValue)]
        [InlineData(typeof(byte[]), CommandOptionType.MultipleValue)]
        [InlineData(typeof(short[]), CommandOptionType.MultipleValue)]
        [InlineData(typeof(int[]), CommandOptionType.MultipleValue)]
        [InlineData(typeof(long[]), CommandOptionType.MultipleValue)]
        [InlineData(typeof(uint[]), CommandOptionType.MultipleValue)]
        [InlineData(typeof(ulong[]), CommandOptionType.MultipleValue)]
        [InlineData(typeof(string[]), CommandOptionType.MultipleValue)]
        [InlineData(typeof(ICollection<int>), CommandOptionType.MultipleValue)]
        [InlineData(typeof(IReadOnlyCollection<int>), CommandOptionType.MultipleValue)]
        [InlineData(typeof(IReadOnlyList<int>), CommandOptionType.MultipleValue)]
        [InlineData(typeof(ISet<int>), CommandOptionType.MultipleValue)]
        [InlineData(typeof(IList<string>), CommandOptionType.MultipleValue)]
        [InlineData(typeof(IEnumerable<string>), CommandOptionType.MultipleValue)]
        [InlineData(typeof(List<string>), CommandOptionType.MultipleValue)]
        [InlineData(typeof(HashSet<string>), CommandOptionType.MultipleValue)]
        public void ItInfersOptionType(Type propType, CommandOptionType optionType)
        {
            Assert.Equal(optionType, CommandOptionTypeMapper.Default.GetOptionType(propType));
        }

        [Fact]
        public void ThrowsForUnknownTypes()
        {
            Assert.Throws<ArgumentException>(() => CommandOptionTypeMapper.Default.GetOptionType(typeof(CommandOptionTypeMapperTests)));
        }

        private struct MyValueType { }

        [Fact]
        public void ThrowsForUnknownValueTypes()
        {
            Assert.Throws<ArgumentException>(() => CommandOptionTypeMapper.Default.GetOptionType(typeof(MyValueType)));
        }
    }
}
