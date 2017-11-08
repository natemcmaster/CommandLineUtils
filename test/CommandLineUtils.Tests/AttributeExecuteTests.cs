using System;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class AttributeExecuteTests
    {
        private class VoidExecuteMethodWithNoArgs
        {
            [Option]
            public string Message { get; set; }

            private void OnExecute()
            {
                Assert.Equal("Add attribute parsing", Message);
            }
        }

        [Fact]
        public void ExecutesVoidMethod()
        {
            var rc = CommandLineApplication.Execute<VoidExecuteMethodWithNoArgs>("-m", "Add attribute parsing");

            Assert.Equal(0, rc);
        }
        
        private class IntExecuteMethodWithNoArgs
        {
            [Option]
            public int Count { get; set; }

            private int OnExecute()
            {
                return Count;
            }
        }

        [Fact]
        public void ExecutesIntMethod()
        {
            var rc = CommandLineApplication.Execute<IntExecuteMethodWithNoArgs>("-c", "5");

            Assert.Equal(5, rc);
        }

        private class OverloadExecute
        {
            private int OnExecute() => 0;

            private void OnExecute(string a)
            {}
        }

        [Fact]
        public void ThrowsIfOverloaded()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CommandLineApplication.Execute<OverloadExecute>());
            
            Assert.Equal(Strings.AmbiguousOnExecuteMethod, ex.Message);
        }
        
        private class NoExecuteMethod
        {}
        
        [Fact]
        public void ThrowsIfNoMethod()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CommandLineApplication.Execute<NoExecuteMethod>());
            
            Assert.Equal(Strings.NoOnExecuteMethodFound, ex.Message);
        }

        private class BadReturnType
        {
            private string OnExecute() => null;
        }
        
        [Fact]
        public void ThrowsIfMethodDoesNotReturnVoidOrInt()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CommandLineApplication.Execute<BadReturnType>());
            
            Assert.Equal(Strings.InvalidOnExecuteReturnType, ex.Message);
        }
    }
}
