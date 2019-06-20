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

        [Theory]
        [InlineData("test", "tests,mytest,mycrazytest,tset", "tests,tset,mytest,mycrazytest")]
        [InlineData("push", "pus,psuh", "pus,psuh")]
        public void SortedMatches(string value, string candidates, string expectedOutput)
        {
            Assert.Equal(expectedOutput,
                string.Join(",",
                    StringDistance.GetBestMatchesSorted(
                        StringDistance.DamareuLevenshteinDistance,
                        value,
                        candidates.Split(','),
                        0.33d
                        )
                    )
                );
        }


        [Fact]
        public void MatchingWithNullReturnsNull()
        {
            Assert.Empty(StringDistance.GetBestMatchesSorted(null, "", new[] { "" }, 0));
            Assert.Empty(StringDistance.GetBestMatchesSorted((s, s1) => 1, null, new[] { "" }, 0));
            Assert.Empty(StringDistance.GetBestMatchesSorted((s, s1) => 1, "", null, 0));
        }

        [Theory]
        [InlineData("test", "random,tests,notreturned,mytest,mycrazytest,tset", "tests,tset,mytest,mycrazytest", 0.33d)]
        [InlineData("push", "fatch,pull,pus,clone,psuh", "pus,psuh,pull", 0.33d)]
        [InlineData("push", "fatch,pull,pus,clone,psuh", "pus,psuh", 0.7d)]
        public void MatchesBelowThresholdAreNotReturned(string value, string candidates, string expectedOutput, double treshold)
        {
            Assert.Equal(expectedOutput,
                string.Join(",",
                    StringDistance.GetBestMatchesSorted(
                        StringDistance.DamareuLevenshteinDistance,
                        value,
                        candidates.Split(','),
                        treshold
                    )
                )
            );
        }
    }
}
