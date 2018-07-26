// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class StringDistanceTests
    {
        [Fact]
        public void EqualStringsNoEdits()
        {
            Assert.Equal(0, StringDistance.DamareuLevenshteinDistance("test", "test"));
        }

        [Theory]
        [InlineData(1, "test", "tests")]
        [InlineData(1, "test", "stest")]
        [InlineData(2, "test", "mytest")]
        [InlineData(7, "test", "mycrazytest")]
        public void Additions(int distance, string s1, string s2)
        {
            Assert.Equal(distance, StringDistance.DamareuLevenshteinDistance(s1, s2));
        }

        [Fact]
        public void AdditionsPrependAndAppend()
        {
            Assert.Equal(9, StringDistance.DamareuLevenshteinDistance("test", "mytestiscrazy"));
        }

        [Fact]
        public void AdditionOfRepeatedCharacters()
        {
            Assert.Equal(1, StringDistance.DamareuLevenshteinDistance("test", "teest"));
        }

        [Fact]
        public void Deletion()
        {
            Assert.Equal(1, StringDistance.DamareuLevenshteinDistance("test", "tst"));
        }

        [Fact]
        public void Transposition()
        {
            Assert.Equal(1, StringDistance.DamareuLevenshteinDistance("test", "tset"));
        }

        [Fact]
        public void AdditionWithTransposition()
        {
            Assert.Equal(2, StringDistance.DamareuLevenshteinDistance("test", "tsets"));
        }

        [Theory]
        [InlineData(1, "banana", "banaan")]
        [InlineData(1, "banana", "abnana")]
        [InlineData(2, "banana", "baanaa")]
        public void TranspositionOfRepeatedCharacters(int distance, string s1, string s2)
        {
            Assert.Equal(distance, StringDistance.DamareuLevenshteinDistance(s1, s2));
        }

        [Fact]
        public void EmptyStringsNoEdits()
        {
            Assert.Equal(0, StringDistance.DamareuLevenshteinDistance("", ""));
        }

        [Theory]
        [InlineData(0.6, "app", "apple")]
        [InlineData(0.5, "ban", "banana")]
        public void ThresholdOfToStrings(double threshold, string s1, string s2)
        {
            var length = Math.Max(s1.Length, s2.Length);
            Assert.Equal(threshold,
                StringDistance.NormalizeDistance(StringDistance.DamareuLevenshteinDistance(s1, s2), length));
        }

        [Fact]
        public void ThresholdOfToStringsThatAreEmpty()
        {
            Assert.Equal(0,
                StringDistance.NormalizeDistance(StringDistance.DamareuLevenshteinDistance(string.Empty, string.Empty), 0));
        }
    }
}
