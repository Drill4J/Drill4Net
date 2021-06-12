using System;
using System.Threading;
using Xunit;

namespace Drill4Net.Injector.Core.UnitTests
{
    public class UnitTest1
    {
        private int _myInt;

        [Fact]
        public void Test1()
        {
            _myInt++;
            Assert.Equal(1, _myInt);
        }

        [Fact]
        public void Test2()
        {
            _myInt++;
            Assert.Equal(1, _myInt);
        }

        [Fact]
        public void Test3()
        {
            _myInt++;
            Assert.Equal(1, _myInt);
        }
    }
}
