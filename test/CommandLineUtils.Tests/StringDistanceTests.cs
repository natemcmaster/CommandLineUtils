using System;
using System.Collections.Generic;
using System.Text;
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
        [InlineData(1,"test","tests")]
        [InlineData(1,"test","stest")]
        [InlineData(2,"test", "mytest")]
        [InlineData(7,"test", "mycrazytest")]
        public void Additions(int distance,string s1, string s2)
        {
            Assert.Equal(distance, StringDistance.DamareuLevenshteinDistance(s1,s2));
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
        [InlineData(1,"banana", "banaan")]
        [InlineData(1,"banana", "abnana")]
        [InlineData(2,"banana", "baanaa")]
        public void TranspositionOfRepeatedCharacters(int distance,string s1,string s2)
        {
            Assert.Equal(distance, StringDistance.DamareuLevenshteinDistance(s1,s2));
        }

        [Fact]
        public void EmptyStringsNoEdits()
        {
            Assert.Equal(0, StringDistance.DamareuLevenshteinDistance("", ""));
        }
        [Theory]
        [InlineData(0.6,"app","apple")]
        [InlineData(0.5, "ban", "banana")]
        public void TresholdOfToStrings(double treshold,string s1,string s2)
        {
            var length = s1.Length > s2.Length ? s1.Length : s2.Length;
            Assert.Equal(treshold,StringDistance.NormalizeDistance(StringDistance.DamareuLevenshteinDistance(s1, s2),length));
        }
        [Theory]
        [InlineData(0, "", "")]
        public void TresholdOfToStringsThatAreEmpty(double treshold, string s1, string s2)
        {
            var length = s1.Length > s2.Length ? s1.Length : s2.Length;
            Assert.Equal(treshold, StringDistance.NormalizeDistance(StringDistance.DamareuLevenshteinDistance(s1, s2), length));
        }
    }
}
